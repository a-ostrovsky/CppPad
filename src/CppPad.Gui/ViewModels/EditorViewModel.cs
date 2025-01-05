using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CppAdapter.BuildAndRun;
using CppPad.BuildSystem;
using CppPad.Gui.Input;
using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase, IDisposable
{
    public static class TabIndices
    {
        public const int CompilerOutput = 0;
        public const int ApplicationOutput = 1;
    }

    private readonly IBuildAndRunFacade _buildAndRunFacade;

    private readonly SemaphoreSlim _buildSemaphore = new(1, 1);
    private readonly ScriptLoaderViewModel _scriptLoader;
    private CancellationTokenSource? _buildCancellationTokenSource;
    private string _title = "Untitled";
    private int _selectedTabIndex = TabIndices.CompilerOutput;

    public EditorViewModel(
        ScriptSettingsViewModel scriptSettings,
        ScriptLoaderViewModel scriptLoader,
        IBuildAndRunFacade buildAndRunFacade,
        SourceCodeViewModel sourceCode
    )
    {
        ScriptSettings = scriptSettings;
        _scriptLoader = scriptLoader;
        _buildAndRunFacade = buildAndRunFacade;
        SourceCode = sourceCode;
        scriptSettings.ApplySettings(SourceCode.ScriptDocument.Script.BuildSettings);
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        _buildAndRunFacade.BuildStatusChanged += Builder_BuildStatusChanged;
    }

    public ScriptSettingsViewModel ScriptSettings { get; }

    public static EditorViewModel DesignInstance { get; } =
        new(
            ScriptSettingsViewModel.DesignInstance,
            ScriptLoaderViewModel.DesignInstance,
            new DummyBuildAndRunFacade(),
            SourceCodeViewModel.DesignInstance
        );

    public SourceCodeViewModel SourceCode { get; }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand CloseCommand { get; }

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

    public event EventHandler? CloseRequested;

    public void ApplySettings(CppBuildSettings settings)
    {
        SourceCode.ScriptDocument = SourceCode.ScriptDocument with
        {
            Script = SourceCode.ScriptDocument.Script with { BuildSettings = settings },
        };
    }

    public async Task OpenFileAsync(string fileName)
    {
        var document = await _scriptLoader.LoadAsync(fileName);
        SourceCode.ScriptDocument = document;
        Title = Path.GetFileName(document.FileName) ?? "Untitled";
    }

    public async Task SaveFileAsAsync(string fileName)
    {
        await _scriptLoader.SaveAsync(SourceCode.ScriptDocument, fileName);
        SourceCode.ScriptDocument = SourceCode.ScriptDocument with { FileName = fileName };
        Title = Path.GetFileName(fileName);
    }

    public Task SaveFileAsync()
    {
        if (string.IsNullOrEmpty(SourceCode.ScriptDocument.FileName))
        {
            throw new InvalidOperationException("File name is not set.");
        }

        return _scriptLoader.SaveAsync(
            SourceCode.ScriptDocument,
            SourceCode.ScriptDocument.FileName
        );
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

    public void AddPlaceholderText()
    {
        SourceCode.Content = """
            #include <iostream>

            int main() {
                std::cout << "Hello World!";
                return 0;
            }
            """;
    }
}
