using System;

namespace CppPad.Gui.ViewModels;

public class SourceCodeViewModel : ViewModelBase
{
    private int _currentColumn;
    private int _currentLine;

    private string _content = $"// Type code here{Environment.NewLine}";

    public static SourceCodeViewModel DesignInstance { get; } = new()
    {
        Content = """
                     #include <iostream>
                     void main() {
                         std::cout << "Hello, World!";
                     }
                     """
    };

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
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