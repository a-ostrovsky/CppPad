#region

using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.FileSystem;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.IO;
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

    private readonly DiskFileSystem _fileSystem;

    private readonly IRouter _router;
    private string? _applicationOutput;
    private int _applicationOutputCaretIndex;
    private string? _compilerOutput;
    private int _compilerOutputCaretIndex;

    private Uri? _currentFilePath;
    private OutputIndex _selectedOutputIndex;

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
        DiskFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _router = router;
        _compiler = compiler;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
        SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
    }


    public static EditorViewModel DesignInstance { get; } = new(
        new DummyRouter(),
        new DummyCompiler(),
        new DiskFileSystem()
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

    public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }

    public ToolsetViewModel? Toolset
    {
        get => _toolset;
        set => SetProperty(ref _toolset, value);
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

            await _fileSystem.WriteAllTextAsync(CurrentFileUri.AbsolutePath, SourceCode);
        });
    }

    private Task SaveAsAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var uri = await _router.ShowSaveFileDialogAsync(AppConstants.FileFilter);
            var filePath = uri?.AbsolutePath;
            if (filePath == null)
            {
                return;
            }

            await _fileSystem.WriteAllTextAsync(filePath, SourceCode);
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
            var buildArgs = new BuildArgs
            {
                SourceCode = SourceCode,
                AdditionalBuildArgs = "/EHsc"
            };
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
            SourceCode = await _fileSystem.ReadAllTextAsync(uri.AbsolutePath);
            SetCurrentFilePath(uri);
        });
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

public class EditorViewModelFactory(IServiceProvider provider) : IEditorViewModelFactory
{
    public EditorViewModel Create()
    {
        return provider.GetService<EditorViewModel>();
    }
}