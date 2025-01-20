using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CppAdapter.BuildAndRun;
using CppPad.BuildSystem;
using CppPad.Common;
using CppPad.Gui.AutoCompletion;
using CppPad.Gui.Eventing;
using CppPad.Gui.Input;
using CppPad.LspClient.Model;
using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase, IDisposable
{
    private readonly IBuildAndRunFacade _buildAndRunFacade;

    private readonly SemaphoreSlim _buildSemaphore = new(1, 1);
    private readonly EventBus _eventBus;
    private readonly ScriptLoaderViewModel _scriptLoader;
    private CancellationTokenSource? _buildCancellationTokenSource;

    private bool _isModified;
    private int _selectedTabIndex = TabIndices.CompilerOutput;
    private string _title = "Untitled";

    public EditorViewModel(
        ScriptSettingsViewModel scriptSettings,
        ScriptLoaderViewModel scriptLoader,
        IBuildAndRunFacade buildAndRunFacade,
        IAutoCompletionAdapter autoCompletionAdapter,
        EventBus eventBus
    )
    {
        ScriptSettings = scriptSettings;
        _scriptLoader = scriptLoader;
        _buildAndRunFacade = buildAndRunFacade;
        _eventBus = eventBus;
        SourceCode = new SourceCodeViewModel(autoCompletionAdapter);
        ScriptSettings.ApplySettings(SourceCode.ScriptDocument.Script.BuildSettings);
        CloseCommand = new AsyncRelayCommand(_ =>
            CloseRequested?.InvokeAsync(this, EventArgs.Empty)
        );
        _buildAndRunFacade.BuildStatusChanged += Builder_BuildStatusChanged;
        var scriptDocumentChangeListener = new ScriptDocumentChangeListener(
            OnScriptDocumentUpdated
        );
        SourceCode.AddChangeListener(scriptDocumentChangeListener);
        IsModified = false;
    }

    public bool IsModified
    {
        get => _isModified;
        private set
        {
            if (SetProperty(ref _isModified, value))
            {
                UpdateTitle();
            }
        }
    }

    public ScriptSettingsViewModel ScriptSettings { get; }

    public static EditorViewModel DesignInstance { get; } =
        new(
            ScriptSettingsViewModel.DesignInstance,
            ScriptLoaderViewModel.DesignInstance,
            new DummyBuildAndRunFacade(),
            new DummyAutoCompletionAdapter(),
            new EventBus(new Dialogs())
        );

    public SourceCodeViewModel SourceCode { get; }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public IAsyncCommand CloseCommand { get; }

    public CompilerOutputViewModel CompilerOutput { get; } = new();

    public ApplicationOutputViewModel ApplicationOutput { get; } = new();

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public void Dispose()
    {
        _buildAndRunFacade.BuildStatusChanged -= Builder_BuildStatusChanged;
        _buildSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public void OnClose()
    {
        _eventBus.Publish(new FileClosedEvent(SourceCode.ScriptDocument));
    }

    private void UpdateTitle()
    {
        var fileName = Path.GetFileName(SourceCode.ScriptDocument.FileName) ?? "Untitled";
        Title = IsModified ? $"*{fileName}" : fileName;
    }

    private void Builder_BuildStatusChanged(object? sender, BuildStatusChangedEventArgs e)
    {
        var message = e.BuildStatus switch
        {
            BuildStatus.PreparingEnvironment => "Preparing environment...",
            BuildStatus.Building => "Building...",
            BuildStatus.Succeeded => "Build succeeded.",
            BuildStatus.Failed => "Build failed.",
            BuildStatus.Cancelled => "Build cancelled.",
            _ => throw new ArgumentException("Unknown build status.", nameof(e)),
        };
        CompilerOutput.AddMessage(message);
    }

    public event AsyncEventHandler? CloseRequested;

    public void ApplySettings(CppBuildSettings settings)
    {
        IsModified = true;
        SourceCode.ApplySettings(settings);
        _eventBus.Publish(new SettingsChangedEvent(SourceCode.ScriptDocument));
    }

    public async Task OpenFileAsync(string fileName)
    {
        var document = await _scriptLoader.LoadAsync(fileName);
        SourceCode.ResetDocument(document);
        IsModified = false;
    }

    public async Task SaveFileAsAsync(string fileName)
    {
        await _scriptLoader.SaveAsync(SourceCode.ScriptDocument, fileName);
        SourceCode.ScriptDocument.FileName = fileName;
        IsModified = false;
    }

    public async Task SaveFileAsync()
    {
        if (string.IsNullOrEmpty(SourceCode.ScriptDocument.FileName))
        {
            throw new InvalidOperationException("File name is not set.");
        }

        await _scriptLoader.SaveAsync(
            SourceCode.ScriptDocument,
            SourceCode.ScriptDocument.FileName
        );
        IsModified = false;
    }

    public async Task CancelBuildAndRunAsync()
    {
        var cancellationTokenSource = _buildCancellationTokenSource;
        if (cancellationTokenSource is null)
        {
            return;
        }

        await cancellationTokenSource.CancelAsync();
    }

    public async Task BuildAndRunAsync(BuildMode buildMode)
    {
        if (!await _buildSemaphore.WaitAsync(0))
        {
            throw new InvalidOperationException("Another build is already in progress.");
        }

        _buildCancellationTokenSource = new CancellationTokenSource();

        try
        {
            CompilerOutput.Reset();
            var buildConfiguration = new BuildConfiguration
            {
                ScriptDocument = SourceCode.ScriptDocument,
                BuildMode = buildMode,
                ErrorReceived = (_, args) =>
                {
                    CompilerOutput.AddMessage($"ERR:{args.Data}");
                },
                ProgressReceived = (_, args) =>
                {
                    CompilerOutput.AddMessage(args.Data);
                },
            };
            var buildAndRunConfiguration = new BuildAndRunConfiguration
            {
                BuildConfiguration = buildConfiguration,
                ExeErrorReceived = (_, args) =>
                {
                    ApplicationOutput.AddMessage(args.Data);
                },
                ExeOutputReceived = (_, args) =>
                {
                    ApplicationOutput.AddMessage(args.Data);
                },
            };
            SelectedTabIndex = TabIndices.CompilerOutput;
            _buildAndRunFacade.BuildStatusChanged += ChangeTabWhenBuildStatusChanges;
            await _buildAndRunFacade.BuildAndRunAsync(
                buildAndRunConfiguration,
                _buildCancellationTokenSource.Token
            );
        }
        finally
        {
            _buildAndRunFacade.BuildStatusChanged -= ChangeTabWhenBuildStatusChanges;
            _buildCancellationTokenSource.Dispose();
            _buildCancellationTokenSource = null;
            _buildSemaphore.Release();
        }

        return;

        void ChangeTabWhenBuildStatusChanges(object? sender, BuildStatusChangedEventArgs e)
        {
            if (e.BuildStatus == BuildStatus.Succeeded)
            {
                SelectedTabIndex = TabIndices.ApplicationOutput;
            }
        }
    }

    public void InitializeNewFile()
    {
        _eventBus.Publish(new NewFileEvent(SourceCode.ScriptDocument));
        var wasModified = IsModified;
        SourceCode.ResetContent("""
            #include <iostream>

            int main() {
                std::cout << "Hello World!";
                return 0;
            }
            """);
        IsModified = wasModified;
    }

    private void OnScriptDocumentUpdated(IContentUpdate contentUpdate)
    {
        IsModified = true;
        _eventBus.Publish(new SourceCodeChangedEvent(contentUpdate, false));
    }

    public static class TabIndices
    {
        public const int CompilerOutput = 0;
        public const int ApplicationOutput = 1;
    }
}
