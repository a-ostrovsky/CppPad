#region

using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.Configuration.Interface;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using CppPad.ScriptFile.Interface;
using CppPad.ScriptFileLoader.Interface;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase, IReactiveObject
{
    public enum OutputIndex
    {
        Compiler = 0,
        Application = 1
    }

    private readonly ICompiler _compiler;
    private readonly IRouter _router;
    private readonly IScriptLoader _scriptLoader;

    private string? _applicationOutput;
    private int _applicationOutputCaretIndex;
    private string? _compilerOutput;
    private int _compilerOutputCaretIndex;

    private Uri? _currentFilePath;
    private ScriptSettingsViewModel _scriptSettings = new();
    private OutputIndex _selectedOutputIndex;

    public event EventHandler<GoToLineRequestedEventArgs>? GoToLineRequested;

    private string _sourceCode =
        """
        #include <iostream>

        int main() {
            std::cout << "Hello, World!" << '\n';
            return 0;
        }
        """;

    private string _title = "Untitled";
    private ToolsetViewModel? _toolset;

    public EditorViewModel(
        IRouter router,
        ICompiler compiler,
        IScriptLoader scriptLoader)
    {
        _router = router;
        _compiler = compiler;
        _scriptLoader = scriptLoader;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
        SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        EditScriptSettingsCommand = ReactiveCommand.CreateFromTask(EditScriptSettingsAsync);
        GoToLineCommand = ReactiveCommand.CreateFromTask(GoToLineAsync);
    }

    public static EditorViewModel DesignInstance { get; } = new(
        new DummyRouter(),
        new DummyCompiler(),
        new DummyScriptLoader()
    );

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string SourceCode
    {
        get => _sourceCode;
        set => SetProperty(ref _sourceCode, value);
    }

    public Uri? CurrentFileUri
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

    public ReactiveCommand<Unit, Unit> RunCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> EditScriptSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }

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

    private Task SaveAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            if (CurrentFileUri == null)
            {
                await SaveAsAsync();
                return;
            }

            var script = GetScript();
            await _scriptLoader.SaveAsync(CurrentFileUri.AbsolutePath, script);
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
            }
        });
    }

    private async Task GoToLineAsync()
    {
        var lineCount = SourceCode.Count(c => c == '\n') + 1;
        var line = await _router.ShowInputBoxAsync<int>($"Line Number: (1-{lineCount})");
        if (line != 0 && line <= lineCount)
        {
            GoToLineRequested?.Invoke(this, new GoToLineRequestedEventArgs(line));
        }
    }

    private Task SaveAsAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var uri = await _router.ShowSaveFileDialogAsync(AppConstants.SaveFileFilter);
            var filePath = uri?.AbsolutePath;
            if (filePath == null)
            {
                return;
            }

            var script = GetScript();
            await _scriptLoader.SaveAsync(filePath, script);
            SetCurrentFilePath(uri!);
        });
    }

    private void SetCurrentFilePath(Uri uri)
    {
        CurrentFileUri = uri;
        Title = Path.GetFileName(uri.AbsolutePath);
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
            var executable = await _compiler.BuildAsync(Toolset.ToCompilerToolset(), buildArgs);
            _compiler.CompilerMessageReceived -= OnCompilerOnCompilerMessageReceived;
            executable.OutputReceived += (_, args) => WriteApplicationOutput(args.Output);
            executable.ErrorReceived += (_, args) => WriteApplicationOutput(args.Error);
            executable.ProcessExited += (_, args) => WriteApplicationOutput(
                $"Process exited with code: {args.ExitCode}");
            WriteCompilerOutput(string.Empty);
            SelectedOutputIndex = OutputIndex.Application;
            await executable.RunAsync();
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

    public Task LoadSourceCodeAsync(Uri uri)
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var script = await _scriptLoader.LoadAsync(uri.AbsolutePath);
            SourceCode = script.Content;
            ScriptSettings.CppStandard = script.CppStandard;
            ScriptSettings.AdditionalBuildArgs = script.AdditionalBuildArgs;
            ScriptSettings.AdditionalIncludeDirs = string.Join(Environment.NewLine, script.AdditionalIncludeDirs);
            ScriptSettings.OptimizationLevel = script.OptimizationLevel;
            SetCurrentFilePath(uri);
        });
    }

    private Script GetScript()
    {
        return new Script
        {
            Content = SourceCode,
            CppStandard = ScriptSettings.CppStandard,
            AdditionalBuildArgs = ScriptSettings.AdditionalBuildArgs,
            AdditionalIncludeDirs = ScriptSettings.AdditionalIncludeDirsArray,
            OptimizationLevel = ScriptSettings.OptimizationLevel
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
            PreBuildCommand = ScriptSettings.PreBuildCommand
        };
        return buildArgs;
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