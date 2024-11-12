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

    public async Task UpdateSettingsAsync(ScriptDocument scriptDocument)
    {
        await _scriptLoader.CreateCppFileAsync(scriptDocument);
        var settings = CreateScriptDocumentSettings(scriptDocument);

        //TODO: Don't use dict of anonymous types.
        await _requestSender.SendDidChangeConfigurationAsync(settings);
    }

    public async Task OpenFileAsync(ScriptDocument scriptDocument)
    {
        await EnsureInitializedAsync();

        await _scriptLoader.CreateCppFileAsync(scriptDocument);
        var path = _scriptLoader.GetCppFilePath(scriptDocument);
        var content = scriptDocument.Script.Content;
        var settings = CreateScriptDocumentSettings(scriptDocument);

        await _requestSender.SendDidOpenAsync(path, content);
        _documentVersions[path] = 1;

        await _requestSender.SendDidChangeConfigurationAsync(settings);
    }

    public async Task<AutoCompletionItem[]> GetCompletionsAsync(
        ScriptDocument scriptDocument, int line, int character)
    {
        var path = _scriptLoader.GetCppFilePath(scriptDocument);
        await EnsureInitializedAsync();

        var requestId = await _requestSender.SendCompletionRequestAsync(path, line, character);
        return await _responseReceiver.ReadCompletionsAsync(requestId);
    }

    public async Task CloseFileAsync(ScriptDocument scriptDocument)
    {
        await EnsureInitializedAsync();
        var path = _scriptLoader.GetCppFilePath(scriptDocument);
        await _requestSender.SendDidCloseAsync(path);
        _documentVersions.Remove(path);
    }

    public async Task UpdateContentAsync(ScriptDocument scriptDocument)
    {
        await EnsureInitializedAsync();
        var path = _scriptLoader.GetCppFilePath(scriptDocument);

        var version = GetNextDocumentVersion(path);
        await _requestSender.SendDidChangeAsync(path, version, scriptDocument.Script.Content);
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