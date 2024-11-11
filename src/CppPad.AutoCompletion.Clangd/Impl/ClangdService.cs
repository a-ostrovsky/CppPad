#region

using System.Text.Json;
using CppPad.AutoCompletion.Clangd.Interface;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.ScriptFile.Interface;
using Range = CppPad.AutoCompletion.Interface.Range;

#endregion

namespace CppPad.AutoCompletion.Clangd.Impl;

public class ClangdService : IAutoCompletionService, IAsyncDisposable
{
    private readonly ILspClient _client;
    private readonly Dictionary<string, int> _documentVersions = new();
    private readonly Lazy<Task<ServerCapabilities>> _initializeTask;
    private readonly string _rootUri;
    private readonly IScriptLoader _scriptLoader;

    public ClangdService(IScriptLoader scriptLoader, ILspClient lspClient)
    {
        _scriptLoader = scriptLoader;
        _client = lspClient;
        _rootUri = AppConstants.TempFolder;
        _client.OnServerNotification += HandleServerNotification;
        _initializeTask = new Lazy<Task<ServerCapabilities>>(InitializeAsync,
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

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return _initializeTask.Value;
    }

    public async Task UpdateSettingsAsync(ScriptDocument scriptDocument)
    {
        await _scriptLoader.CreateCppFileAsync(scriptDocument);
        var settings = CreateScriptDocumentSettings(scriptDocument);

        //TODO: Don't use dict of anonymous types.
        await SendDidChangeConfigurationAsync(settings);
    }

    private Dictionary<string, object> CreateScriptDocumentSettings(ScriptDocument scriptDocument)
    {
        var path = _scriptLoader.GetCppFilePath(scriptDocument);

        var cppStandard = scriptDocument.Script.CppStandard switch
        {
            CppStandard.Cpp11 => "c++11",
            CppStandard.Cpp14 => "c++14",
            CppStandard.Cpp17 => "c++17",
            CppStandard.Cpp20 => "c++20",
            CppStandard.Cpp23 => "c++23",
            CppStandard.CppLatest => "c++2c",
            _ => "c++2c"
        };
        var settings = new Dictionary<string, object>
        {
            [path] = new
            {
                workingDirectory = AppConstants.TempFolder,
                compilationCommand = new[] { "clang++", $"-std={cppStandard}", "-c", path }
            }
        };
        return settings;
    }

    public async Task OpenFileAsync(ScriptDocument scriptDocument)
    {
        await EnsureInitializedAsync();

        await _scriptLoader.CreateCppFileAsync(scriptDocument);
        var path = _scriptLoader.GetCppFilePath(scriptDocument);
        var content = scriptDocument.Script.Content;
        var settings = CreateScriptDocumentSettings(scriptDocument);

        await SendDidOpenAsync(path, content);
        _documentVersions[path] = 1;
        
        await SendDidChangeConfigurationAsync(settings);
    }

    public async Task<AutoCompletionItem[]> GetCompletionsAsync(
        ScriptDocument scriptDocument, int line, int character)
    {
        var path = _scriptLoader.GetCppFilePath(scriptDocument);
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
                    uri = PathToUriFormat(path)
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

    private static string PathToUriFormat(string path)
    {
        return $"file:///{path.Replace('\\', '/')}";
    }

    public async Task CloseFileAsync(ScriptDocument scriptDocument)
    {
        await EnsureInitializedAsync();
        var path = _scriptLoader.GetCppFilePath(scriptDocument);

        var uri = PathToUriFormat(path);
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

    public async Task UpdateContentAsync(ScriptDocument scriptDocument)
    {
        await EnsureInitializedAsync();
        var path = _scriptLoader.GetCppFilePath(scriptDocument);
        
        var uri = PathToUriFormat(path);
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
                    new { text = scriptDocument.Script.Content }
                }
            }
        };

        await _client.SendMessageAsync(didChangeNotification);
    }

    private async Task SendDidChangeConfigurationAsync(IDictionary<string, object> settings)
    {
        var didChangeConfigurationNotification = new
        {
            jsonrpc = "2.0",
            method = "workspace/didChangeConfiguration",
            @params = new
            {
                settings = new
                {
                    compilationDatabaseChanges = settings
                }
            }
        };
        await _client.SendMessageAsync(didChangeConfigurationNotification);
    }

    private async Task SendDidOpenAsync(string path, string content)
    {
        var didOpenNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didOpen",
            @params = new
            {
                textDocument = new
                {
                    uri = PathToUriFormat(path),
                    languageId = "cpp",
                    version = 1,
                    text = content
                }
            }
        };
        await _client.SendMessageAsync(didOpenNotification);
    }

    public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;

    private async Task EnsureInitializedAsync()
    {
        await _initializeTask.Value;
    }

    private async Task<ServerCapabilities> InitializeAsync()
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
                processId = Environment.ProcessId,
                rootUri = _rootUri,
                capabilities = new { },
                initializationOptions = new
                {
                    fallbackFlags = new[] { "-std=c++20" }
                }
            }
        };
        await _client.SendMessageAsync(initRequest);
        var capabilitiesResponse = await _client.ReadResponseAsync(requestId);
        var capabilities = ParseCapabilities(capabilitiesResponse);

        var initializedNotification = new
        {
            jsonrpc = "2.0",
            method = "initialized",
            @params = new { }
        };
        await _client.SendMessageAsync(initializedNotification);
        return capabilities;
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
        var method = args.Message.RootElement.GetProperty("method").GetString() ?? "EMPTY_METHOD";

        if (method == "textDocument/publishDiagnostics")
        {
            var @params = args.Message.RootElement.GetProperty("params");
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

    private static ServerCapabilities ParseCapabilities(JsonDocument? response)
    {
        if (response == null)
        {
            return new ServerCapabilities();
        }

        // TODO: Parse errors
        var capabilities = new ServerCapabilities();
        if (response.RootElement.TryGetProperty("result", out var resultElement) &&
            resultElement.TryGetProperty("capabilities", out var capabilitiesElement) &&
            capabilitiesElement.TryGetProperty("completionProvider",
                out var completionProviderElement) &&
            completionProviderElement.TryGetProperty("triggerCharacters",
                out var triggerCharactersElement))
        {
            var triggerCharacters = triggerCharactersElement.EnumerateArray()
                .Select(tc => tc.GetString())
                .Where(tc => !string.IsNullOrEmpty(tc))
                .Select(tc => tc![0])
                .ToHashSet();

            capabilities = new ServerCapabilities
            {
                TriggerCharacters = triggerCharacters
            };
        }

        return capabilities;
    }

    private static AutoCompletionItem[] ParseCompletions(JsonDocument? response)
    {
        //TODO: Parse errors
        if (response == null)
        {
            return [];
        }

        var items = response.RootElement.GetProperty("result").GetProperty("items");

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