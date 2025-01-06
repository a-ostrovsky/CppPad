#region

using System.Text.Json;
using CppPad.LspClient.Model;

#endregion

namespace CppPad.LspClient;

public class ResponseReceiver : IDisposable
{
    private readonly LspProxy _proxy;

    public ResponseReceiver(LspProxy proxy)
    {
        _proxy = proxy;
        _proxy.OnServerNotification += HandleServerNotification;
    }

    public void Dispose()
    {
        _proxy.OnServerNotification -= HandleServerNotification;
        GC.SuppressFinalize(this);
    }

    public async Task<PositionInFile[]> ReadDefinitionsAsync(int requestId)
    {
        var response = await _proxy.ReadResponseAsync(requestId);
        return ParseDefinitions(response);
    }

    public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;

    public async Task<ServerCapabilities> ReadCapabilitiesAsync(int requestId)
    {
        var response = await _proxy.ReadResponseAsync(requestId);
        return ParseCapabilities(response);
    }

    public async Task<AutoCompletionItem[]> ReadCompletionsAsync(int requestId)
    {
        var response = await _proxy.ReadResponseAsync(requestId);
        return ParseCompletions(response);
    }

    private static PositionInFile[] ParseDefinitions(JsonDocument? response)
    {
        if (response == null)
        {
            return [];
        }

        var resultElement = response.RootElement.GetProperty("result");

        var positions = resultElement
            .EnumerateArray()
            .Select(item => new PositionInFile
            {
                FileName = new Uri(item.GetProperty("uri").GetString() ?? string.Empty).LocalPath,
                Position = new Position
                {
                    Line = item.GetProperty("range")
                        .GetProperty("start")
                        .GetProperty("line")
                        .GetInt32(),
                    Character = item.GetProperty("range")
                        .GetProperty("start")
                        .GetProperty("character")
                        .GetInt32(),
                },
            })
            .ToArray();

        return positions;
    }

    private static ServerCapabilities ParseCapabilities(JsonDocument? response)
    {
        if (response == null)
        {
            return new ServerCapabilities();
        }

        var capabilities = new ServerCapabilities();
        if (
            response.RootElement.TryGetProperty("result", out var resultElement)
            && resultElement.TryGetProperty("capabilities", out var capabilitiesElement)
            && capabilitiesElement.TryGetProperty(
                "completionProvider",
                out var completionProviderElement
            )
            && completionProviderElement.TryGetProperty(
                "triggerCharacters",
                out var triggerCharactersElement
            )
        )
        {
            var triggerCharacters = triggerCharactersElement
                .EnumerateArray()
                .Select(tc => tc.GetString())
                .Where(tc => !string.IsNullOrEmpty(tc))
                .Select(tc => tc![0])
                .ToHashSet();

            capabilities = new ServerCapabilities { TriggerCharacters = triggerCharacters };
        }

        return capabilities;
    }

    private static AutoCompletionItem[] ParseCompletions(JsonDocument? response)
    {
        // TODO: Parse errors

        if (response == null)
        {
            return [];
        }

        var resultElement = response.RootElement.GetProperty("result");

        // Check if 'items' property exists
        if (!resultElement.TryGetProperty("items", out var itemsElement))
        {
            return [];
        }

        var completions = itemsElement
            .EnumerateArray()
            .Select(item => new AutoCompletionItem
            {
                Label = TryParseLabel(item) ?? string.Empty,
                Documentation = TryParseDocumentation(item),
                Priority = ParseScore(item),
                Edits = ParseEdits(item),
            })
            .OrderBy(item => item.Label)
            .ToArray();

        return completions;
    }

    private static SourceCodeRange ParseRange(JsonElement rangeElement)
    {
        var start = rangeElement.GetProperty("start");
        var end = rangeElement.GetProperty("end");

        return new SourceCodeRange
        {
            Start = new Position
            {
                Line = start.GetProperty("line").GetInt32(),
                Character = start.GetProperty("character").GetInt32(),
            },
            End = new Position
            {
                Line = end.GetProperty("line").GetInt32(),
                Character = end.GetProperty("character").GetInt32(),
            },
        };
    }

    private static Diagnostic[] ParseDiagnostics(JsonElement diagnosticsJson)
    {
        var diagnostics = new List<Diagnostic>();

        foreach (var diag in diagnosticsJson.EnumerateArray())
        {
            var range = diag.GetProperty("range");
            var messageText = diag.GetProperty("message").GetString() ?? "EMPTY_MESSAGE";
            var severity = diag.TryGetProperty("severity", out var severityElement)
                ? (DiagnosticSeverity)severityElement.GetInt32()
                : DiagnosticSeverity.Information;

            diagnostics.Add(
                new Diagnostic
                {
                    Range = ParseRange(range),
                    Message = messageText,
                    Severity = severity,
                }
            );
        }

        return diagnostics.ToArray();
    }

    private static string? TryParseLabel(JsonElement item)
    {
        return item.GetProperty("label").GetString();
    }

    private static double ParseScore(JsonElement item)
    {
        if (item.TryGetProperty("score", out var scoreElement))
        {
            return scoreElement.GetDouble();
        }

        // Default priority if 'score' is not available
        return 0.0;
    }

    private static string? TryParseDocumentation(JsonElement item)
    {
        if (item.TryGetProperty("documentation", out var documentation))
        {
            if (documentation.ValueKind == JsonValueKind.String)
            {
                return documentation.GetString();
            }

            if (documentation.TryGetProperty("value", out var value))
            {
                return value.GetString();
            }
        }

        return null;
    }

    private static List<Edit> ParseEdits(JsonElement item)
    {
        var edits = new List<Edit>();

        if (item.TryGetProperty("textEdit", out var textEdit))
        {
            var range = textEdit.GetProperty("range");
            var newText = textEdit.GetProperty("newText").GetString() ?? string.Empty;
            edits.Add(new Edit(ParseRange(range), newText));
        }

        if (item.TryGetProperty("additionalTextEdits", out var additionalTextEdits))
        {
            var additionalEdits = additionalTextEdits
                .EnumerateArray()
                .Select(edit => new Edit(
                    ParseRange(edit.GetProperty("range")),
                    edit.GetProperty("newText").GetString() ?? string.Empty
                ));
            edits.AddRange(additionalEdits);
        }

        return edits;
    }

    private void HandleServerNotification(object? sender, ServerNotificationEventArgs args)
    {
        var method = args.Message.RootElement.GetProperty("method").GetString() ?? "EMPTY_METHOD";

        if (method == "textDocument/publishDiagnostics")
        {
            var @params = args.Message.RootElement.GetProperty("params");
            var uri = @params.GetProperty("uri").GetString() ?? "http://EMPTY_URI";
            var diagnosticsJson = @params.GetProperty("diagnostics");

            var diagnostics = ParseDiagnostics(diagnosticsJson);

            OnDiagnosticsReceived?.Invoke(
                this,
                new DiagnosticsReceivedEventArgs(new Uri(uri), diagnostics.ToArray())
            );
        }
        // Handle other notification types if needed
    }
}
