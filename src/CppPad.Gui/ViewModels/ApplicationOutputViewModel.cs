namespace CppPad.Gui.ViewModels;

public class ApplicationOutputViewModel : ViewModelBase
{
    private string _applicationOutput = string.Empty;

    public string ApplicationOutput
    {
        get => _applicationOutput;
        set => SetProperty(ref _applicationOutput, value);
    }
}