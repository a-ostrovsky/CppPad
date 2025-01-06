﻿using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class SourceCodeViewModel : ViewModelBase
{
    private string _content = string.Empty;
    private int _currentColumn;
    private int _currentLine;
    private ScriptDocument _scriptDocument = new();

    public static SourceCodeViewModel DesignInstance { get; } =
        new()
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

    public ScriptDocument ScriptDocument
    {
        get
        {
            _scriptDocument = _scriptDocument with
            {
                Script = _scriptDocument.Script with { Content = Content }
            };
            return _scriptDocument;
        }
        set
        {
            if (SetProperty(ref _scriptDocument, value))
            {
                Content = _scriptDocument.Script.Content;
            }
        }
    }
}