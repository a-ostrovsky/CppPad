using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CppPad.Gui.Input;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;
using CppPad.SystemAdapter.IO;

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
        SourceCode.ScriptDocument = document;
        Title = Path.GetFileName(document.FileName) ?? "Untitled";
    }

    public async Task SaveFileAsAsync(string fileName)
    {
        await _loader.SaveAsync(SourceCode.ScriptDocument, fileName);
        SourceCode.ScriptDocument = SourceCode.ScriptDocument with { FileName = fileName };
        Title = Path.GetFileName(fileName);
    }

    public Task SaveFileAsync()
    {
        if (string.IsNullOrEmpty(SourceCode.ScriptDocument.FileName))
        {
            throw new InvalidOperationException("File name is not set.");
        }

        return _loader.SaveAsync(SourceCode.ScriptDocument, SourceCode.ScriptDocument.FileName);
    }
}