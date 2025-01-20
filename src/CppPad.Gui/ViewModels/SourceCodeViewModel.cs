using System.Collections.Generic;
using AvaloniaEdit;
using CppPad.Gui.AutoCompletion;
using CppPad.LspClient.Model;
using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class SourceCodeViewModel(IAutoCompletionAdapter autoCompletionAdapter) : ViewModelBase
{
    private readonly List<ScriptDocumentChangeListener> _changeListeners = [];
    private int _currentColumn;
    private int _currentLine;
    private ScriptDocument _scriptDocument = new();
    private string _content = string.Empty;

    public static SourceCodeViewModel DesignInstance { get; } =
        new(new DummyAutoCompletionAdapter())
        {
            ScriptDocument = new ScriptDocument
            {
                Script = new ScriptData
                {
                    Content = """
                              #include <iostream>
                              void main() {
                                  std::cout << "Hello, World!";
                              }
                              """
                }
            }
        };

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
                Script = _scriptDocument.Script with { Content = _content },
            };
            return _scriptDocument;
        }
        set
        {
            if (SetProperty(ref _scriptDocument, value))
            {
                _content = _scriptDocument.Script.Content;
            }
        }
    }

    public void InstallAutoCompletion(TextEditor textEditor)
    {
        autoCompletionAdapter.Attach(textEditor, this);
    }

    public void UninstallAutoCompletion()
    {
        autoCompletionAdapter.Detach();
    }

    public void ApplySettings(CppBuildSettings settings)
    {
        ScriptDocument = ScriptDocument with
        {
            Script = ScriptDocument.Script with { BuildSettings = settings }
        };
    }

    public void ResetDocument(ScriptDocument scriptDocument)
    {
        ScriptDocument = scriptDocument;
        foreach (var changeListener in _changeListeners)
        {
            changeListener.Reset(ScriptDocument);
        }
    }
    
    public void ResetContent(string updatedContent)
    {
        _content = updatedContent;
        foreach (var changeListener in _changeListeners)
        {
            changeListener.Reset(ScriptDocument);
        }
    }

    public void SetContent(string updatedContent, Range range, string? insertedText = null)
    {
        _content = updatedContent;
        foreach (var changeListener in _changeListeners)
        {
            changeListener.EditText(ScriptDocument, range, insertedText);
        }
    }

    public void AddChangeListener(ScriptDocumentChangeListener changeListener)
    {
        _changeListeners.Add(changeListener);
    }

    public void RemoveChangeListener(ScriptDocumentChangeListener changeListener)
    {
        _changeListeners.Remove(changeListener);
    }
}