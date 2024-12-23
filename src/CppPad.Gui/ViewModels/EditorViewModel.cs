using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CppPad.FileSystem;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase
{
    private readonly ScriptLoader _loader;
    private string _title = "Untitled";

    public EditorViewModel(ScriptLoader loader, SourceCodeViewModel sourceCode)
    {
        _loader = loader;
        SourceCode = sourceCode;
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public static EditorViewModel DesignInstance { get; } =
        new(new ScriptLoader(new ScriptSerializer(), new DiskFileSystem()), 
            SourceCodeViewModel.DesignInstance);

    public SourceCodeViewModel SourceCode { get; }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand CloseCommand { get; }

    public event EventHandler? CloseRequested;

    public async Task OpenFileAsync(string fileName)
    {
        var document = await _loader.LoadAsync(fileName);
        SourceCode.Content = document.Script.Content;
        Title = Path.GetFileName(document.FileName) ?? "Untitled";
    }
}