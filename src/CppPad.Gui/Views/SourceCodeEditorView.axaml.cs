#region

using System;
using System.Diagnostics;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CppPad.Gui.ViewModels;
using TextMateSharp.Grammars;

#endregion

namespace CppPad.Gui.Views;

public partial class SourceCodeEditorView : UserControl
{
    private bool _isInternalChange;

    public SourceCodeEditorView()
    {
        InitializeComponent();
        Init();
    }

    public void ScrollToLine(int line)
    {
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        textEditor.ScrollToLine(line);
        textEditor.CaretOffset = GetCaretOffsetForLine(textEditor, line);
    }

    private void Init()
    {
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = textEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp")
                .Id));
        
        textEditor.TextChanged += TextEditor_TextChanged;
        DataContextChanged += SourceCodeEditorView_DataContextChanged;
    }

    private void SourceCodeEditorView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        if (DataContext is not EditorViewModel vm)
        {
            throw new InvalidOperationException("DataContext is not EditorViewModel");
        }
        
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
    
        textEditor.Text = vm.SourceCode;
        
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EditorViewModel.SourceCode) && !_isInternalChange)
            {
                textEditor.Text = vm.SourceCode;
            }
        };
        
        var autoCompletionProvider = new AutoCompletionProvider(vm);
        autoCompletionProvider.Attach(textEditor);
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        try
        {
            _isInternalChange = true;
            ((EditorViewModel)DataContext!).SourceCode = ((TextEditor)sender!).Text;
        }
        finally
        {
            _isInternalChange = false;
        }
    }

    private static int GetCaretOffsetForLine(TextEditor textEditor, int line)
    {
        var document = textEditor.Document;
        var lineInfo = document.GetLineByNumber(line);
        return lineInfo.Offset;
    }
}