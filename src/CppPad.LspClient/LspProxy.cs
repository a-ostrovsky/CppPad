#region

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using CppPad.Common;
using CppPad.Logging;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.LspClient;

public class LspProxy(ILspProcess lspProcess)
{
    private readonly ILogger _logger = Log.CreateLogger<LspProxy>();
    private readonly BlockingCollection<JsonDocument> _responseQueue = new();

    private readonly AsyncLock _writeLock = new();
    private int _requestId = 1;

    public event EventHandler<ServerNotificationEventArgs>? OnServerNotification;

    public int GetNextRequestId()
    {
        return _requestId++;
    }

    public async Task SendMessageAsync(object message)
    {
        var inputWriter = lspProcess.InputWriter;
        if (inputWriter == null)
        {
            throw new InvalidOperationException("LspClient is not initialized.");
        }

        var json = JsonSerializer.Serialize(message);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        using var lck = await _writeLock.LockAsync();
        await inputWriter.WriteAsync($"Content-Length: {jsonBytes.Length}\r\n\r\n");
        await inputWriter.WriteAsync(json);
        await inputWriter.FlushAsync();
    }

    public async Task<JsonDocument?> ReadResponseAsync(int expectedId)
    {
        return await Task.Run(() =>
        {
            while (true)
            {
                var response = _responseQueue.Take();
                if (
                    response.RootElement.TryGetProperty("id", out var idElement)
                    && idElement.GetInt32() == expectedId
                )
                {
                    return response;
                }
            }
        });
    }

    public async Task InitializeAsync()
    {
        using var lck = await _writeLock.LockAsync();
        await lspProcess.StartAsync();
        StartListening();
    }

    private async Task<string?> ReadMessageAsync()
    {
        if (lspProcess.OutputReader == null)
        {
            throw new InvalidOperationException("LspClient is not initialized.");
        }

        string? line;
        var contentLength = 0;

        // Read headers
        while (!string.IsNullOrEmpty(line = await lspProcess.OutputReader.ReadLineAsync()))
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
        var read = await lspProcess.OutputReader.ReadAsync(buffer, 0, contentLength);
        return new string(buffer, 0, read);
    }

    private void StartListening()
    {
        Task.Run(async () =>
        {
            while (!lspProcess.HasExited)
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
                    OnServerNotification?.Invoke(this, new ServerNotificationEventArgs(jsonDoc));
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
