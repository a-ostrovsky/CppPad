#region

using CppPad.LspClient;
using CppPad.LspClient.Model;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.SystemAdapter.IO;

#endregion

namespace CppPad.CodeAssistance;

public class CodeAssistant : ICodeAssistant, IDisposable
{
    private readonly Dictionary<string, int> _documentVersions = new();
    private readonly Lazy<Task<ServerCapabilities>> _initializeTask;
    private readonly RequestSender _requestSender;
    private readonly ResponseReceiver _responseReceiver;
    private readonly string _rootUri;
    private readonly ScriptLoader _scriptLoader;

    public CodeAssistant(
        DiskFileSystem fileSystem,
        ScriptLoader scriptLoader,
        ResponseReceiver responseReceiver,
        RequestSender requestSender
    )
    {
        _scriptLoader = scriptLoader;
        _responseReceiver = responseReceiver;
        _requestSender = requestSender;
        _rootUri = fileSystem.SpecialFolders.TempFolder;
        _responseReceiver.OnDiagnosticsReceived += HandleOnDiagnosticsReceived;
        _initializeTask = new Lazy<Task<ServerCapabilities>>(
            InitializeAsync,
            LazyThreadSafetyMode.ExecutionAndPublication
        );
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return _initializeTask.Value;
    }

    public async Task UpdateSettingsAsync(ScriptDocument document)
    {
        await _scriptLoader.CreateCppFileAsync(document);
        var settings = await CreateScriptDocumentSettingsAsync(document);

        await _requestSender.SendDidChangeConfigurationAsync(settings);
    }

    public async Task OpenFileAsync(ScriptDocument document)
    {
        await EnsureInitializedAsync();

        await _scriptLoader.CreateCppFileAsync(document);
        var path = await _scriptLoader.CreateCppFileAsync(document);
        var content = document.Script.Content;
        var settings = await CreateScriptDocumentSettingsAsync(document);

        await _requestSender.SendDidOpenAsync(path, content);
        _documentVersions[path] = 1;

        await _requestSender.SendDidChangeConfigurationAsync(settings);
    }

    public async Task<AutoCompletionItem[]> GetCompletionsAsync(
        ScriptDocument document,
        Position position
    )
    {
        var path = await _scriptLoader.CreateCppFileAsync(document);
        await EnsureInitializedAsync();

        var positionInFile = new PositionInFile { FileName = path, Position = position };

        var requestId = await _requestSender.SendCompletionRequestAsync(positionInFile);
        return await _responseReceiver.ReadCompletionsAsync(requestId);
    }

    public async Task<PositionInFile[]> GetDefinitionsAsync(
        ScriptDocument document,
        Position position
    )
    {
        var path = await _scriptLoader.CreateCppFileAsync(document);
        await EnsureInitializedAsync();

        var positionInFile = new PositionInFile { FileName = path, Position = position };

        var requestId = await _requestSender.SendFindDefinitionAsync(positionInFile);
        return await _responseReceiver.ReadDefinitionsAsync(requestId);
    }

    public async Task CloseFileAsync(ScriptDocument document)
    {
        await EnsureInitializedAsync();
        var path = await _scriptLoader.CreateCppFileAsync(document);
        await _requestSender.SendDidCloseAsync(path);
        _documentVersions.Remove(path);
    }

    public async Task UpdateContentAsync(ScriptDocument document)
    {
        await EnsureInitializedAsync();
        var path = await _scriptLoader.CreateCppFileAsync(document);

        var version = GetNextDocumentVersion(path);
        await _requestSender.SendDidChangeAsync(path, version, document.Script.Content);
    }

    public void Dispose()
    {
        _responseReceiver.OnDiagnosticsReceived -= HandleOnDiagnosticsReceived;
        GC.SuppressFinalize(this);
    }

    private void HandleOnDiagnosticsReceived(object? sender, DiagnosticsReceivedEventArgs e)
    {
        OnDiagnosticsReceived?.Invoke(this, e);
    }

    private async Task<Dictionary<string, object>> CreateScriptDocumentSettingsAsync(
        ScriptDocument document
    )
    {
        var path = await _scriptLoader.CreateCppFileAsync(document);

        var cppStandard = document.Script.BuildSettings.CppStandard switch
        {
            CppStandard.Cpp11 => "c++11",
            CppStandard.Cpp14 => "c++14",
            CppStandard.Cpp17 => "c++17",
            CppStandard.Cpp20 => "c++20",
            CppStandard.Cpp23 => "c++23",
            CppStandard.CppLatest => "c++2c",
            _ => "c++2c",
        };
        var settings = new Dictionary<string, object>
        {
            [path] = new
            {
                workingDirectory = _rootUri,
                compilationCommand = new[] { "clang++", $"-std={cppStandard}", "-c", path },
            },
        };
        return settings;
    }

    public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;

    private async Task EnsureInitializedAsync()
    {
        await _initializeTask.Value;
    }

    private async Task<ServerCapabilities> InitializeAsync()
    {
        await _requestSender.InitializeClientAsync();
        var requestId = await _requestSender.SendInitializeRequestAsync(
            Environment.ProcessId,
            _rootUri
        );
        var capabilities = await _responseReceiver.ReadCapabilitiesAsync(requestId);
        await _requestSender.SendInitializedNotificationAsync();
        return capabilities;
    }

    private int GetNextDocumentVersion(string fileName)
    {
        if (_documentVersions.TryGetValue(fileName, out var version))
        {
            version++;
            _documentVersions[fileName] = version;
        }
        else
        {
            version = 1;
            _documentVersions.Add(fileName, version);
        }

        return version;
    }
}
