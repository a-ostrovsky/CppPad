#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.Configuration.Interface;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using CppPad.ScriptFile.Interface;
using ReactiveUI;
using ITimer = CppPad.Common.ITimer;

#endregion

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase, IReactiveObject
{
    public enum OutputIndex
    {
        Compiler = 0,
        Application = 1
    }

    private const int MaxTitleLength = 13;
    private readonly ThrottledAutoCompletionUpdater _autoCompletionUpdater;

    private readonly ICompiler _compiler;
    private readonly IConfigurationStore _configurationStore;
    private readonly IDefinitionsWindowViewModelFactory _definitionsWindowViewModelFactory;
    private readonly IRouter _router;
    private readonly IScriptLoader _scriptLoader;
    private readonly TemplatesViewModel _templatesViewModel;

    private string? _applicationOutput;
    private int _applicationOutputCaretIndex;
    private string? _compilerOutput;
    private int _compilerOutputCaretIndex;

    private string? _currentFilePath;

    private Identifier _currentIdentifier = IdGenerator.GenerateUniqueId();

    private bool _isModified;
    private ScriptSettingsViewModel _scriptSettings = new();
    private OutputIndex _selectedOutputIndex;

    private string _sourceCode = string.Empty;
    private string _title = string.Empty;
    private ToolsetViewModel? _toolset;

    // TODO: there are too many arguments here, consider refactoring
    public EditorViewModel(
        IDefinitionsWindowViewModelFactory definitionsWindowViewModelFactory,
        TemplatesViewModel templatesViewModel,
        IRouter router,
        ICompiler compiler,
        IScriptLoader scriptLoader,
        IConfigurationStore configurationStore,
        IAutoCompletionService autoCompletionService,
        ITimer timer)
    {
        _definitionsWindowViewModelFactory = definitionsWindowViewModelFactory;
        _templatesViewModel = templatesViewModel;
        _router = router;
        _compiler = compiler;
        _scriptLoader = scriptLoader;
        _configurationStore = configurationStore;
        _autoCompletionUpdater = new ThrottledAutoCompletionUpdater(timer, autoCompletionService);
        AutoCompletionService = autoCompletionService;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
        GoToDefinitionsCommand = ReactiveCommand.Create(GoToDefinitions);
        SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        EditScriptSettingsCommand = ReactiveCommand.CreateFromTask(EditScriptSettingsAsync);
        GoToLineCommand = ReactiveCommand.CreateFromTask(GoToLineAsync);
        SaveAsTemplateCommand = ReactiveCommand.CreateFromTask(SaveAsTemplateAsync);
    }

    public static EditorViewModel DesignInstance { get; } = new(
        new DummyDefinitionsWindowViewModelFactory(),
        new TemplatesViewModel(new DummyTemplateLoader()),
        new DummyRouter(),
        new DummyCompiler(),
        new DummyScriptLoader(),
        new DummyConfigurationStore(),
        new DummyAutoCompletionService(),
        new DummyTimer()
    );

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public IAutoCompletionService AutoCompletionService { get; }

    public string SourceCode
    {
        get => _sourceCode;
        set
        {
            SetPropertyAndUpdateModified(ref _sourceCode, value);
            _autoCompletionUpdater.SetDocument(GetCurrentScriptDocument());
        }
    }

    public string? CurrentFilePath
    {
        get => _currentFilePath;
        set => SetProperty(ref _currentFilePath, value);
    }

    public string? ApplicationOutput
    {
        get => _applicationOutput;
        set => SetProperty(ref _applicationOutput, value);
    }

    public string? CompilerOutput
    {
        get => _compilerOutput;
        set => SetProperty(ref _compilerOutput, value);
    }

    public int ApplicationOutputCaretIndex
    {
        get => _applicationOutputCaretIndex;
        set => SetProperty(ref _applicationOutputCaretIndex, value);
    }

    public int CompilerOutputCaretIndex
    {
        get => _compilerOutputCaretIndex;
        set => SetProperty(ref _compilerOutputCaretIndex, value);
    }

    public OutputIndex SelectedOutputIndex
    {
        get => _selectedOutputIndex;
        set => SetProperty(ref _selectedOutputIndex, value);
    }

    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (SetProperty(ref _isModified, value))
            {
                UpdateTitle();
            }
        }
    }

    public ReactiveCommand<Unit, Unit> RunCommand { get; }

    public ReactiveCommand<Unit, Unit> GoToDefinitionsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> EditScriptSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveAsTemplateCommand { get; }

    public ReactiveCommand<Unit, Unit> GoToLineCommand { get; }

    public ToolsetViewModel? Toolset
    {
        get => _toolset;
        set => SetProperty(ref _toolset, value);
    }

    public ScriptSettingsViewModel ScriptSettings
    {
        get => _scriptSettings;
        set => SetProperty(ref _scriptSettings, value);
    }


    public void RaisePropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }

    public void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    public async Task InitAsNewFileAsync()
    {
        try
        {
            _autoCompletionUpdater.PauseUpdate();
            _sourceCode =
                """
                #include <iostream>

                int main() {
                    std::cout << "Hello, World!" << '\n';
                    return 0;
                }
                """;
        }
        finally
        {
            _autoCompletionUpdater.ResumeUpdate();
        }

        // Don't set the property directly to avoid setting IsModified flag.
        OnPropertyChanged(nameof(SourceCode));

        Title = "Untitled";
        await AutoCompletionService.OpenFileAsync(GetScriptDocument(null));
    }

    public event EventHandler<GoToLineRequestedEventArgs>? GoToLineRequested;

    public event EventHandler<EventArgs>? GoToDefinitionsRequested;

    private static string TruncateTitle(string title)
    {
        if (title.Length <= MaxTitleLength)
        {
            return title;
        }

        const int charsToShow = MaxTitleLength - 3; //-3 for the ellipsis
        const int frontChars = charsToShow / 2;
        const int backChars = charsToShow - frontChars;

        return $"{title[..frontChars]}...{title[^backChars..]}";
    }

    private void UpdateTitle()
    {
        var baseTitle = _currentFilePath != null
            ? TruncateTitle(Path.GetFileName(_currentFilePath))
            : "Untitled";
        Title = IsModified ? $"{baseTitle}*" : baseTitle;
    }

    public Task SaveAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            if (CurrentFilePath == null)
            {
                await SaveAsAsync();
                return;
            }

            var scriptDocument = GetCurrentScriptDocument();
            await _scriptLoader.SaveAsync(scriptDocument);
            await _configurationStore.SaveLastOpenedFileNameAsync(CurrentFilePath);
            IsModified = false;
        });
    }

    private Task EditScriptSettingsAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var settings = Cloner.DeepCopy(ScriptSettings);
            Debug.Assert(settings != null);
            var vm = new ScriptSettingsWindowViewModel(settings);
            await _router.ShowDialogAsync(vm);
            if (vm.ShouldApplySettings)
            {
                ScriptSettings = vm.ScriptSettings;
                await AutoCompletionService.UpdateSettingsAsync(GetCurrentScriptDocument());
                IsModified = true;
            }
        });
    }

    private async Task GoToLineAsync()
    {
        var lineCount = SourceCode.Count(c => c == '\n') + 1;
        var line = await _router.ShowInputBoxAsync<int>($"Line Number: (1-{lineCount})");
        if (line != 0 && line <= lineCount)
        {
            GoToLineRequested?.Invoke(this, new GoToLineRequestedEventArgs(line, 0));
        }
    }

    private Task SaveAsAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var uri = await _router.ShowSaveFileDialogAsync(AppConstants.SaveFileFilter);
            var path = uri?.LocalPath;
            if (path == null)
            {
                return;
            }

            var scriptDocument = GetScriptDocument(path);
            await _scriptLoader.SaveAsync(scriptDocument);
            SetCurrentFilePath(path);
            await _configurationStore.SaveLastOpenedFileNameAsync(path);
            IsModified = false;
        });
    }

    private Task SaveAsTemplateAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var templateName = await _router.ShowInputBoxAsync<string>("Enter template name:");
            if (templateName == null)
            {
                return;
            }

            var filePath = Path.Combine(AppConstants.TemplateFolder,
                templateName + AppConstants.DefaultFileExtension);

            var script = GetScript();
            await _templatesViewModel.SaveAsync(filePath, script);
        });
    }

    private void SetCurrentFilePath(string path)
    {
        CurrentFilePath = path;
        UpdateTitle();
    }

    private Task RunAsync()
    {
        if (Toolset == null)
        {
            return Task.CompletedTask;
        }

        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            SelectedOutputIndex = OutputIndex.Compiler;
            CompilerOutput = string.Empty;
            ApplicationOutput = string.Empty;
            _compiler.CompilerMessageReceived += OnCompilerOnCompilerMessageReceived;
            var buildArgs = GetBuildArgs();
            try
            {
                var executable = await _compiler.BuildAsync(Toolset.ToCompilerToolset(), buildArgs);
                executable.OutputReceived += (_, args) => WriteApplicationOutput(args.Output);
                executable.ErrorReceived += (_, args) => WriteApplicationOutput(args.Error);
                executable.ProcessExited += (_, args) => WriteApplicationOutput(
                    $"Process exited with code: {args.ExitCode}");
                WriteCompilerOutput(string.Empty);
                SelectedOutputIndex = OutputIndex.Application;
                executable.SetAdditionalEnvironmentPaths(
                    ScriptSettings.AdditionalEnvironmentPathsArray);
                await executable.RunAsync();
            }
            finally
            {
                _compiler.CompilerMessageReceived -= OnCompilerOnCompilerMessageReceived;
            }
        });
    }

    private void OnCompilerOnCompilerMessageReceived(object? _, CompilerMessageEventArgs args)
    {
        WriteCompilerOutput(args.Message);
    }

    private void WriteApplicationOutput(string text)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            ApplicationOutput += text + Environment.NewLine;
            ApplicationOutputCaretIndex = ApplicationOutput.Length; // Scroll to the end
        });
    }

    private void WriteCompilerOutput(string text)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            CompilerOutput += text + Environment.NewLine;
            CompilerOutputCaretIndex = CompilerOutput.Length; // Scroll to the end
        });
    }

    public Task LoadSourceCodeAsync(string path)
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var scriptDocument = await _scriptLoader.LoadAsync(path);
            SetScript(scriptDocument.Script);
            _currentIdentifier = scriptDocument.Identifier;
            SetCurrentFilePath(path);
            await AutoCompletionService.OpenFileAsync(GetScriptDocument(path));
        });
    }

    public Task InitFromTemplateAsync(string templateName)
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var script = await _templatesViewModel.LoadAsync(templateName);
            SetScript(script);
            await AutoCompletionService.OpenFileAsync(GetScriptDocument(null));
        });
    }

    private void SetPropertyAndUpdateModified<T>(ref T field, T value)
    {
        if (SetProperty(ref field, value))
        {
            IsModified = true;
        }
    }

    private void SetScript(Script script)
    {
        try
        {
            _autoCompletionUpdater.PauseUpdate();
            SourceCode = script.Content;
        }
        finally
        {
            _autoCompletionUpdater.ResumeUpdate();
        }

        ScriptSettings.CppStandard = script.CppStandard;
        ScriptSettings.AdditionalBuildArgs = script.AdditionalBuildArgs;
        ScriptSettings.PreBuildCommand = script.PreBuildCommand;
        ScriptSettings.AdditionalIncludeDirs =
            string.Join(Environment.NewLine, script.AdditionalIncludeDirs);
        ScriptSettings.StaticallyLinkedLibraries =
            string.Join(Environment.NewLine, script.StaticallyLinkedLibraries);
        ScriptSettings.LibrarySearchPaths =
            string.Join(Environment.NewLine, script.LibrarySearchPaths);
        ScriptSettings.AdditionalEnvironmentPaths =
            string.Join(Environment.NewLine, script.AdditionalEnvironmentPaths);
        ScriptSettings.OptimizationLevel = script.OptimizationLevel;
    }

    public ScriptDocument GetCurrentScriptDocument()
    {
        return GetScriptDocument(CurrentFilePath);
    }

    private ScriptDocument GetScriptDocument(string? fileName)
    {
        return new ScriptDocument
        {
            FileName = fileName,
            Identifier = _currentIdentifier,
            Script = GetScript()
        };
    }

    public Script GetScript()
    {
        return new Script
        {
            Content = SourceCode,
            CppStandard = ScriptSettings.CppStandard,
            AdditionalBuildArgs = ScriptSettings.AdditionalBuildArgs,
            PreBuildCommand = ScriptSettings.PreBuildCommand,
            AdditionalIncludeDirs = ScriptSettings.AdditionalIncludeDirsArray,
            OptimizationLevel = ScriptSettings.OptimizationLevel,
            AdditionalEnvironmentPaths = ScriptSettings.AdditionalEnvironmentPathsArray,
            LibrarySearchPaths = ScriptSettings.LibrarySearchPathsArray,
            StaticallyLinkedLibraries = ScriptSettings.StaticallyLinkedLibrariesArray
        };
    }

    private BuildArgs GetBuildArgs()
    {
        var buildArgs = new BuildArgs
        {
            SourceCode = SourceCode,
            AdditionalBuildArgs = ScriptSettings.AdditionalBuildArgs + " /EHsc",
            CppStandard = ScriptSettings.CppStandard,
            OptimizationLevel = ScriptSettings.OptimizationLevel,
            AdditionalIncludeDirs = ScriptSettings.AdditionalIncludeDirsArray,
            LibrarySearchPaths = ScriptSettings.LibrarySearchPathsArray,
            StaticallyLinkedLibraries = ScriptSettings.StaticallyLinkedLibrariesArray,
            PreBuildCommand = ScriptSettings.PreBuildCommand
        };
        return buildArgs;
    }

    private void GoToDefinitions()
    {
        GoToDefinitionsRequested?.Invoke(this, EventArgs.Empty);
    }

    public async Task GoToDefinitionsAsync(PositionInFile[] definitions)
    {
        if (definitions.Length == 0)
        {
            return;
        }

        if (definitions.Length == 1)
        {
            var currentDocumentPath = _scriptLoader.GetCppFilePath(GetCurrentScriptDocument());
            if (currentDocumentPath == definitions[0].FileName)
            {
                var line = definitions[0].Position.Line + 1;
                var character = definitions[0].Position.Character;
                GoToLineRequested?.Invoke(this, new GoToLineRequestedEventArgs(line, character));
                return;
            }
        }

        var definitionsViewModel = _definitionsWindowViewModelFactory.Create();
        await definitionsViewModel.DefinitionsViewModel.SetDefinitionsAsync(definitions);
        await _router.ShowDialogAsync(definitionsViewModel);
    }

    private class ThrottledAutoCompletionUpdater : IAsyncDisposable
    {
        private static readonly TimeSpan ThrottleTime = TimeSpan.FromMilliseconds(1000);
        private readonly IAutoCompletionService _autoCompletionService;
        private readonly Lock _documentLock = new();
        private readonly ITimer _timer;
        private ScriptDocument? _document;
        private bool _pauseUpdates;

        public ThrottledAutoCompletionUpdater(ITimer timer,
            IAutoCompletionService autoCompletionService)
        {
            _timer = timer;
            _autoCompletionService = autoCompletionService;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Change(ThrottleTime, Timeout.InfiniteTimeSpan);
        }

        public ValueTask DisposeAsync()
        {
            _timer.Elapsed -= TimerOnElapsed;
            return _timer is IAsyncDisposable asyncDisposable
                ? asyncDisposable.DisposeAsync()
                : ValueTask.CompletedTask;
        }


        private async void TimerOnElapsed(object? sender, EventArgs e)
        {
            ScriptDocument? document;
            lock (_documentLock)
            {
                document = _document;
                _document = null;
            }

            if (document == null)
            {
                _timer.Change(ThrottleTime, Timeout.InfiniteTimeSpan);
                return;
            }

            await ErrorHandler.Instance.RunWithErrorHandlingAsync(() =>
            {
                try
                {
                    return _autoCompletionService.UpdateContentAsync(document);
                }
                finally
                {
                    _timer.Change(ThrottleTime, Timeout.InfiniteTimeSpan);
                }
            });
        }

        public void PauseUpdate()
        {
            _pauseUpdates = true;
        }

        public void ResumeUpdate()
        {
            _pauseUpdates = false;
        }

        public void SetDocument(ScriptDocument document)
        {
            lock (_documentLock)
            {
                if (_pauseUpdates)
                {
                    return;
                }

                _document = document;
            }
        }
    }
}

public interface IEditorViewModelFactory
{
    EditorViewModel Create();
}

public class DummyEditorViewModelFactory : IEditorViewModelFactory
{
    public EditorViewModel Create()
    {
        return EditorViewModel.DesignInstance;
    }
}

public class EditorViewModelFactory(
    IServiceProvider provider,
    IConfigurationStore configurationStore) : IEditorViewModelFactory
{
    public EditorViewModel Create()
    {
        var vm = provider.GetService<EditorViewModel>();
        var config = configurationStore.GetToolsetConfiguration();
        var defaultToolset =
            config.Toolsets.SingleOrDefault(toolset => toolset.Id == config.DefaultToolsetId);
        var toolsetViewModel = defaultToolset != null ? new ToolsetViewModel(defaultToolset) : null;
        vm.Toolset = toolsetViewModel;
        return vm;
    }
}