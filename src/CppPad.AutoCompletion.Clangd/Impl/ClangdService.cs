#region

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using CppPad.AutoCompletion.Clangd.Interface;
using CppPad.AutoCompletion.Interface;
using Range = CppPad.AutoCompletion.Interface.Range;

#endregion

namespace CppPad.AutoCompletion.Clangd.Impl;

public class ClangdService : IAutoCompletionService, IAsyncDisposable
{
    private readonly ILspClient _client;
    private readonly Dictionary<string, int> _documentVersions = new();
    private readonly Lazy<Task> _initializeTask;
    private readonly string _rootUri;

    public ClangdService(ILspClient lspClient)
    {
        _client = lspClient;
        _rootUri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                           string.Empty).AbsoluteUri;
        _client.OnServerNotification += HandleServerNotification;
        _initializeTask = new Lazy<Task>(InitializeAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is IAsyncDisposable disposableClient)
        {
            await disposableClient.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;

    public async Task OpenFileAsync(string filePath, string fileContent)
    {
        await EnsureInitializedAsync();

        var uri = $"file:///{filePath.Replace('\\', '/')}";
        var didOpenNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didOpen",
            @params = new
            {
                textDocument = new
                {
                    uri,
                    languageId = "cpp",
                    version = 1,
                    text = fileContent
                }
            }
        };
        await _client.SendMessageAsync(didOpenNotification);
        _documentVersions[uri] = 1;
    }

    public async Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line,
        int character)
    {
        await EnsureInitializedAsync();

        var requestId = _client.GetNextRequestId();
        var completionRequest = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "textDocument/completion",
            @params = new
            {
                textDocument = new
                {
                    uri = $"file:///{filePath.Replace('\\', '/')}"
                },
                position = new
                {
                    line,
                    character
                }
            }
        };
        await _client.SendMessageAsync(completionRequest);

        var response = await _client.ReadResponseAsync(requestId);
        return ParseCompletions(response);
    }

    public async Task DidChangeAsync(string filePath, string newText)
    {
        await EnsureInitializedAsync();

        var uri = $"file:///{filePath.Replace('\\', '/')}";
        var version = GetNextDocumentVersion(uri);

        var didChangeNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didChange",
            @params = new
            {
                textDocument = new
                {
                    uri,
                    version
                },
                contentChanges = new[]
                {
                    new { text = newText }
                }
            }
        };

        await _client.SendMessageAsync(didChangeNotification);
    }

    public async Task CloseFileAsync(string filePath)
    {
        await EnsureInitializedAsync();

        var uri = $"file:///{filePath.Replace('\\', '/')}";
        var didCloseNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didClose",
            @params = new
            {
                textDocument = new
                {
                    uri
                }
            }
        };
        await _client.SendMessageAsync(didCloseNotification);
        _documentVersions.Remove(uri);
    }

    public async Task RenameFileAsync(string oldFilePath, string newFilePath)
    {
        await EnsureInitializedAsync();

        var oldUri = $"file:///{oldFilePath.Replace('\\', '/')}";
        var newUri = $"file:///{newFilePath.Replace('\\', '/')}";

        var didRenameFilesNotification = new
        {
            jsonrpc = "2.0",
            method = "workspace/didRenameFiles",
            @params = new
            {
                files = new[]
                {
                    new
                    {
                        oldUri,
                        newUri
                    }
                }
            }
        };

        await _client.SendMessageAsync(didRenameFilesNotification);

        // Update the document versions dictionary
        if (_documentVersions.Remove(oldUri, out var value))
        {
            _documentVersions[newUri] = value;
        }
    }

    private async Task EnsureInitializedAsync()
    {
        await _initializeTask.Value;
    }

    private async Task InitializeAsync()
    {
        await _client.InitializeAsync();
        var requestId = _client.GetNextRequestId();
        var initRequest = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "initialize",
            @params = new
            {
                processId = Process.GetCurrentProcess().Id,
                rootUri = _rootUri,
                capabilities = new { }
            }
        };
        await _client.SendMessageAsync(initRequest);
        await _client.ReadResponseAsync(requestId);

        var initializedNotification = new
        {
            jsonrpc = "2.0",
            method = "initialized",
            @params = new { }
        };
        await _client.SendMessageAsync(initializedNotification);
    }

    private int GetNextDocumentVersion(string uri)
    {
        if (_documentVersions.TryGetValue(uri, out var version))
        {
            version++;
            _documentVersions[uri] = version;
        }
        else
        {
            version = 1;
            _documentVersions.Add(uri, version);
        }

        return version;
    }

    private void HandleServerNotification(object? sender, ServerNotificationEventArgs args)
    {
        var jsonDoc = JsonDocument.Parse(args.Message);
        var method = jsonDoc.RootElement.GetProperty("method").GetString() ?? "EMPTY_METHOD";

        if (method == "textDocument/publishDiagnostics")
        {
            var @params = jsonDoc.RootElement.GetProperty("params");
            var uri = @params.GetProperty("uri").GetString() ?? "http://EMPTY_URI";
            var diagnosticsJson = @params.GetProperty("diagnostics");

            var diagnostics = new List<Diagnostic>();

            foreach (var diag in diagnosticsJson.EnumerateArray())
            {
                var range = diag.GetProperty("range");
                var messageText = diag.GetProperty("message").GetString() ?? "EMPTY_MESSAGE";
                var severity = diag.TryGetProperty("severity", out var severityElement)
                    ? (DiagnosticSeverity)severityElement.GetInt32()
                    : DiagnosticSeverity.Information;

                diagnostics.Add(new Diagnostic
                {
                    Range = ParseRange(range),
                    Message = messageText,
                    Severity = severity
                });
            }

            OnDiagnosticsReceived?.Invoke(this,
                new DiagnosticsReceivedEventArgs(new Uri(uri), diagnostics.ToArray()));
        }
        // Handle other notification types if needed
    }

    private static Range ParseRange(JsonElement rangeElement)
    {
        var start = rangeElement.GetProperty("start");
        var end = rangeElement.GetProperty("end");

        return new Range
        {
            Start = new Position
            {
                Line = start.GetProperty("line").GetInt32(),
                Character = start.GetProperty("character").GetInt32()
            },
            End = new Position
            {
                Line = end.GetProperty("line").GetInt32(),
                Character = end.GetProperty("character").GetInt32()
            }
        };
    }

    private static AutoCompletionItem[] ParseCompletions(string? response)
    {
        //TODO: Parse errors
        if (string.IsNullOrEmpty(response))
        {
            return Array.Empty<AutoCompletionItem>();
        }

        using var jsonDoc = JsonDocument.Parse(response);
        var items = jsonDoc.RootElement.GetProperty("result").GetProperty("items");

        var completions = items.EnumerateArray()
            .Select(item => new AutoCompletionItem
            {
                Label = TryParseLabel(item) ?? string.Empty,
                Documentation = TryParseDocumentation(item),
                Priority = ParseScore(item),
                Edits = ParseEdits(item)
            })
            .OrderBy(item => item.Label)
            .ToArray();

        return completions;
    }

    private static string? TryParseLabel(JsonElement item)
    {
        return item.GetProperty("label").GetString();
    }
    
    private static double ParseScore(JsonElement item)
    {
        return item.GetProperty("score").GetDouble();
    }

    private static string? TryParseDocumentation(JsonElement item)
    {
        if (!item.TryGetProperty("documentation", out var documentation) ||
            !documentation.TryGetProperty("value", out var value))
        {
            return null;
        }

        return value.GetString() ?? string.Empty;
    }


    private static IReadOnlyList<Edit> ParseEdits(JsonElement item)
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
            var additionalEdits = additionalTextEdits.EnumerateArray()
                .Select(edit => new Edit(ParseRange(edit.GetProperty("range")),
                    edit.GetProperty("newText").GetString() ?? string.Empty));
            edits.AddRange(additionalEdits);
        }

        return edits;
    }
}