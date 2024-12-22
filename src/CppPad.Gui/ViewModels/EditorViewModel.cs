using System;
using System.Windows.Input;
using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase
{
    private string _title = "Untitled";

    public EditorViewModel(SourceCodeViewModel sourceCode)
    {
        SourceCode = sourceCode;
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public static EditorViewModel DesignInstance { get; } = new(SourceCodeViewModel.DesignInstance);

    public SourceCodeViewModel SourceCode { get; }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand CloseCommand { get; }
    
    public event EventHandler? CloseRequested;
}