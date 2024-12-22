namespace CppPad.Gui.ViewModels;

public class EditorViewModel(SourceCodeViewModel sourceCode) : ViewModelBase
{
    private string _title = "Untitled";
    public static EditorViewModel DesignInstance { get; } = new(SourceCodeViewModel.DesignInstance);

    public SourceCodeViewModel SourceCode => sourceCode;

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }
}