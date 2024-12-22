namespace CppPad.Gui.ViewModels;

public class SourceCodeViewModel : ViewModelBase
{
    private int _currentColumn;
    private int _currentLine;

    private string _sourceCode = "// Type code here";

    public static SourceCodeViewModel DesignInstance { get; } = new()
    {
        SourceCode = """
                     #include <iostream>
                     void main() {
                         std::cout << "Hello, World!";
                     }
                     """
    };

    public string SourceCode
    {
        get => _sourceCode;
        set => SetProperty(ref _sourceCode, value);
    }

    public int CurrentLine
    {
        get => _currentLine;
        set => SetProperty(ref _currentLine, value);
    }

    public int CurrentColumn
    {
        get => _currentColumn;
        set => SetProperty(ref _currentColumn, value);
    }
}