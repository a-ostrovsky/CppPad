using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CppPad.AutoCompletion.Clangd.Interface;
using CppPad.Common;
using Microsoft.Extensions.Logging;

namespace CppPad.AutoCompletion.Clangd.Impl
{
    public class LspClient(ILoggerFactory loggerFactory) : ILspClient, IAsyncDisposable
    {
        private static readonly string ClangdPath =
            Path.Combine(AppConstants.ClangdFolder, "bin", "clangd.exe");
        
        private readonly ILogger _logger = loggerFactory.CreateLogger<LspClient>();

        private readonly AsyncLock _writeLock = new();
        private readonly Process _process = new()
        {
            StartInfo = new ProcessStartInfo(ClangdPath)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                // Uncomment for verbose logging
                //Arguments = "--log=verbose"
            }
        };

        private StreamWriter? _inputWriter;
        private StreamReader? _outputReader;
        private int _requestId = 1;
        private readonly BlockingCollection<JsonDocument> _responseQueue = new();

        public async ValueTask DisposeAsync()
        {
            if (!_process.HasExited)
            {
                _process.Kill();
                _process.Dispose();
            }

            if (_inputWriter != null)
            {
                await _inputWriter.DisposeAsync();
            }

            _outputReader?.Dispose();

            GC.SuppressFinalize(this);
        }

        public event EventHandler<ServerNotificationEventArgs>? OnServerNotification;

        public int GetNextRequestId()
        {
            return _requestId++;
        }

        public async Task SendMessageAsync(object message)
        {
            if (_inputWriter == null)
            {
                throw new InvalidOperationException("LspClient is not initialized.");
            }

            var json = JsonSerializer.Serialize(message);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            using var lck = await _writeLock.LockAsync();
            await _inputWriter.WriteAsync($"Content-Length: {jsonBytes.Length}\r\n\r\n");
            await _inputWriter.WriteAsync(json);
            await _inputWriter.FlushAsync();
        }

        public async Task<JsonDocument?> ReadResponseAsync(int expectedId)
        {
            return await Task.Run(() =>
            {
                while (true)
                {
                    var response = _responseQueue.Take();
                    if (response.RootElement.TryGetProperty("id", out var idElement) &&
                        idElement.GetInt32() == expectedId)
                    {
                        return response;
                    }
                }
            });
        }

        public async Task InitializeAsync()
        {
            using var lck = await _writeLock.LockAsync();
            await Task.Run(() =>
            {
                _process.Start();

                _inputWriter =
                    new StreamWriter(_process.StandardInput.BaseStream, new UTF8Encoding(false))
                        { AutoFlush = true };
                _outputReader =
                    new StreamReader(_process.StandardOutput.BaseStream, new UTF8Encoding(false));

                StartListening();
            });
        }

        private async Task<string?> ReadMessageAsync()
        {
            if (_outputReader == null)
            {
                throw new InvalidOperationException("LspClient is not initialized.");
            }

            string? line;
            var contentLength = 0;
            
            // Read headers
            while (!string.IsNullOrEmpty(line = await _outputReader.ReadLineAsync()))
            {
                if (line.StartsWith("Content-Length:"))
                {
                    contentLength = int.Parse(line["Content-Length:".Length..].Trim());
                }
            }

            if (contentLength <= 0)
            {
                return null;
            }

            var buffer = new char[contentLength];
            var read = await _outputReader.ReadAsync(buffer, 0, contentLength);
            return new string(buffer, 0, read);
        }

        private void StartListening()
        {
            Task.Run(async () =>
            {
                while (!_process.HasExited)
                {
                    var message = await ReadMessageAsync();
                    _logger.LogInformation("Received message: {message}", message);
                    if (message == null)
                    {
                        continue;
                    }

                    var jsonDoc = JsonDocument.Parse(message);
                    if (jsonDoc.RootElement.TryGetProperty("method", out _))
                    {
                        // It's a server notification
                        OnServerNotification?.Invoke(this,
                            new ServerNotificationEventArgs(jsonDoc));
                    }
                    else
                    {
                        // It's a response
                        _responseQueue.Add(jsonDoc);
                    }
                }
            });
        }
    }
}