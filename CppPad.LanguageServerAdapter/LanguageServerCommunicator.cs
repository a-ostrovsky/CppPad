#region

using CppPad.LanguageServerAdapter.Requests;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

#endregion

namespace CppPad.LanguageServerAdapter;

public class LanguageServerCommunicator(
    StreamWriter writer,
    StreamReader reader,
    ILoggerFactory loggerFactor)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger _logger = loggerFactor.CreateLogger<LanguageServerCommunicator>();

    public async Task SendRequestAsync(IRequest request, CancellationToken token = default)
    {
        var json = request.ToJson();
        _logger.LogInformation("Sending request: <{json}>", json);
        var content = Encoding.UTF8.GetBytes(json);
        var header = $"Content-Length: {content.Length}\r\n\r\n";
        await writer.WriteAsync(header);
        await writer.WriteAsync(json);
        await writer.FlushAsync(token);
    }

    public async Task<T?> ReadResponseAsync<T>(CancellationToken token = default)
    {
        var contentLength = 0;
        while (true)
        {
            var line = await reader.ReadLineAsync(token);
            if (line == null)
            {
                throw new LanguageServerAdapterException("Failed to read response");
            }

            if (line.StartsWith("Content-Length:"))
            {
                var contentLengthString = line["Content-Length:".Length..].Trim();
                contentLength = int.Parse(contentLengthString);
            }

            if (line == string.Empty)
            {
                break;
            }
        }

        var buffer = new char[contentLength];
        await reader.ReadAsync(buffer, 0, contentLength);
        var json = new string(buffer);
        _logger.LogInformation("Received response: <{json}>", json);
        var result = JsonSerializer.Deserialize<T>(json, SerializerOptions);
        return result;
    }
}