#region

using CppPad.AutoCompletion.Clangd.Interface;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.AutoCompletion.Clangd.Impl;

public class ClangdService : IAutoCompletionService, IDisposable
{
    private readonly Dictionary<string, int> _documentVersions = new();
    private readonly Lazy<Task<ServerCapabilities>> _initializeTask;
    private readonly IRequestSender _requestSender;
    private readonly IResponseReceiver _responseReceiver;
    private readonly string _rootUri;
    private readonly IScriptLoader _scriptLoader;

    public ClangdService(
        IScriptLoader scriptLoader, IResponseReceiver responseReceiver,
        IRequestSender requestSender)
    {
        _scriptLoader = scriptLoader;
        _responseReceiver = responseReceiver;
        _requestSender = requestSender;
        _rootUri = AppConstants.TempFolder;
        _responseReceiver.OnDiagnosticsReceived += HandleOnDiagnosticsReceived;
        _initializeTask = new Lazy<Task<ServerCapabilities>>(InitializeAsync,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return _initializeTask.Value;
    }

    public async Task UpdateSettingsAsync(ScriptDocument document)
    {
        await _scriptLoader.CreateCppFileAsync(document);
        var settings = CreateScriptDocumentSettings(document);

        //TODO: Don't use dict of anonymous types.
        await _requestSender.SendDidChangeConfigurationAsync(settings);
    }

    public async Task OpenFileAsync(ScriptDocument document)
    {
        await EnsureInitializedAsync();

        await _scriptLoader.CreateCppFileAsync(document);
        var path = _scriptLoader.GetCppFilePath(document);
        var content = document.Script.Content;
        var settings = CreateScriptDocumentSettings(document);

        await _requestSender.SendDidOpenAsync(path, content);
        _documentVersions[path] = 1;

        await _requestSender.SendDidChangeConfigurationAsync(settings);
    }

    public async Task<AutoCompletionItem[]> GetCompletionsAsync(
        ScriptDocument document, Position position)
    {
        var path = _scriptLoader.GetCppFilePath(document);
        await EnsureInitializedAsync();

        var positionInFile = new PositionInFile
        {
            FileName = path,
            Position = position
        };
        
        var requestId =
            await _requestSender.SendCompletionRequestAsync(positionInFile);
        return await _responseReceiver.ReadCompletionsAsync(requestId);
    }

    public async Task<PositionInFile[]> GetDefinitionsAsync(ScriptDocument document, Position position)
    {
        var path = _scriptLoader.GetCppFilePath(document);
        await EnsureInitializedAsync();

        var positionInFile = new PositionInFile
        {
            FileName = path,
            Position = position
        };
        
        var requestId =
            await _requestSender.SendFindDefinitionAsync(positionInFile);
        return await _responseReceiver.ReadDefinitionsAsync(requestId);
    }

    public async Task CloseFileAsync(ScriptDocument document)
    {
        await EnsureInitializedAsync();
        var path = _scriptLoader.GetCppFilePath(document);
        await _requestSender.SendDidCloseAsync(path);
        _documentVersions.Remove(path);
    }

    public async Task UpdateContentAsync(ScriptDocument document)
    {
        await EnsureInitializedAsync();
        var path = _scriptLoader.GetCppFilePath(document);

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

    private Dictionary<string, object> CreateScriptDocumentSettings(ScriptDocument document)
    {
        var path = _scriptLoader.GetCppFilePath(document);

        var cppStandard = document.Script.CppStandard switch
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

    public event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;

    private async Task EnsureInitializedAsync()
    {
        await _initializeTask.Value;
    }

    private async Task<ServerCapabilities> InitializeAsync()
    {
        await _requestSender.InitializeClientAsync();
        var requestId =
            await _requestSender.SendInitializeRequestAsync(Environment.ProcessId, _rootUri);
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