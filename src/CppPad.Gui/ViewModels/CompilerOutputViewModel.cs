namespace CppPad.Gui.ViewModels;

public class CompilerOutputViewModel : ViewModelBase
{
    private string _compilerOutput = string.Empty;

    public string CompilerOutput
    {
        get => _compilerOutput;
        set => SetProperty(ref _compilerOutput, value);
    }
}