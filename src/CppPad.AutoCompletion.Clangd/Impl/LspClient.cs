#region

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CppPad.AutoCompletion.Clangd.Interface;
using CppPad.Common;

#endregion

namespace CppPad.AutoCompletion.Clangd.Impl
{
    public class LspClient : ILspClient, IAsyncDisposable
    {
        private static readonly string ClangdPath =
            Path.Combine(AppConstants.ClangdFolder, "bin", "clangd.exe");

        private readonly AsyncLock _lock = new();

        private readonly Process _process = new()
        {
            StartInfo = new ProcessStartInfo(ClangdPath)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = "--log=verbose"
            }
        };

        private StreamWriter? _inputWriter;
        private StreamReader? _outputReader;
        private int _requestId = 1;

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
            using var lck = await _lock.LockAsync();
            await _inputWriter.WriteAsync($"Content-Length: {jsonBytes.Length}\r\n\r\n");
            await _inputWriter.WriteAsync(json);
            await _inputWriter.FlushAsync();
        }

        public async Task<string?> ReadResponseAsync(int expectedId)
        {
            while (true)
            {
                var response = await ReadMessageAsync();
                if (response == null)
                {
                    continue;
                }

                var jsonDoc = JsonDocument.Parse(response);
                if (jsonDoc.RootElement.TryGetProperty("id", out var idElement) &&
                    idElement.GetInt32() == expectedId)
                {
                    return response;
                }
            }
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                _process.Start();

                _inputWriter =
                    new StreamWriter(_process.StandardInput.BaseStream, new UTF8Encoding(false))
                        { AutoFlush = true };
                _outputReader =
                    new StreamReader(_process.StandardOutput.BaseStream, new UTF8Encoding(false));

                //StartListening();
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

            using var lck = await _lock.LockAsync();

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

        // TODO: This is not yet used. It's for handling server notifications which will 
        // be implemented later.
        private void StartListening()
        {
            Task.Run(async () =>
            {
                while (!_process.HasExited)
                {
                    var message = await ReadMessageAsync();
                    if (message != null)
                    {
                        var jsonDoc = JsonDocument.Parse(message);
                        if (jsonDoc.RootElement.TryGetProperty("method", out _))
                        {
                            // It's a server notification
                            OnServerNotification?.Invoke(this,
                                new ServerNotificationEventArgs(message));
                        }
                        // Handle response messages if necessary
                    }
                }
            });
        }
    }
}