#region

using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CppPad.Common;
using CppPad.CompilerAdapter.Msvc;
using CppPad.FileSystem;
using CppPad.Gui.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    private readonly DiskFileSystem _fileSystem;

    private readonly ILoggerFactory _loggerFactory;
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
        ILoggerFactory loggerFactory,
        IRouter router,
        DiskFileSystem fileSystem)
    {
        _loggerFactory = loggerFactory;
        _fileSystem = fileSystem;
        _router = router;
        RunCommand = ReactiveCommand.CreateFromTask(RunAsync);
        SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAsAsync);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
    }


    public static EditorViewModel DesignInstance { get; } = new(
        NullLoggerFactory.Instance,
        new DummyRouter(),
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

    private async Task SaveAsync()
    {
        if (CurrentFileUri == null)
        {
            await SaveAsAsync();
            return;
        }

        await _fileSystem.WriteAllTextAsync(CurrentFileUri.AbsolutePath, SourceCode);
    }

    private async Task SaveAsAsync()
    {
        var uri = await _router.ShowSaveFileDialogAsync(AppConstants.FileFilter);
        var filePath = uri?.AbsolutePath;
        if (filePath == null)
        {
            return;
        }

        await _fileSystem.WriteAllTextAsync(filePath, SourceCode);
        SetCurrentFilePath(uri!);
    }

    private void SetCurrentFilePath(Uri uri)
    {
        CurrentFileUri = uri;
        Title = Path.GetFileName(uri.AbsolutePath);
    }

    private async Task RunAsync()
    {
        var executablePath = Toolset?.ExecutablePath;
        if (executablePath == null)
        {
            return;
        }

        SelectedOutputIndex = OutputIndex.Compiler;
        CompilerOutput = string.Empty;
        ApplicationOutput = string.Empty;
        var compiler = new Compiler(
            executablePath, _fileSystem, _loggerFactory);
        compiler.CompilerMessage += (_, args) => WriteCompilerOutput("  >>" + args.Message);
        var executable = await compiler.BuildAsync(SourceCode, "/EHsc");
        executable.OutputReceived += (_, args) => WriteApplicationOutput(args.Output);
        executable.ErrorReceived += (_, args) => WriteApplicationOutput(args.Error);
        executable.ProcessExited += (_, args) => WriteApplicationOutput(
            $"Process exited with code: {args.ExitCode}");
        WriteCompilerOutput(string.Empty);
        SelectedOutputIndex = OutputIndex.Application;
        await executable.RunAsync();
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

    public async Task LoadSourceCodeAsync(Uri uri)
    {
        SourceCode = await _fileSystem.ReadAllTextAsync(uri.AbsolutePath);
        SetCurrentFilePath(uri);
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