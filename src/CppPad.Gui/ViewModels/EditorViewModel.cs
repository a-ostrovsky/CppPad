using System;
using System.Windows.Input;

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase
{
    private string _title = "Untitled";
    private readonly SourceCodeViewModel _sourceCode;

    public EditorViewModel(SourceCodeViewModel sourceCode)
    {
        _sourceCode = sourceCode;
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public static EditorViewModel DesignInstance { get; } = new(SourceCodeViewModel.DesignInstance);

    public SourceCodeViewModel SourceCode => _sourceCode;

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand CloseCommand { get; }
    
    public event EventHandler? CloseRequested;
}