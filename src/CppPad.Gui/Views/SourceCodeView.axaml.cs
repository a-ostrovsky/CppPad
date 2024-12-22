#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CppPad.Gui.ViewModels;
using TextMateSharp.Grammars;

#endregion

namespace CppPad.Gui.Views;

public partial class SourceCodeView : UserControl
{
    private bool _isInternalChange;

    private SourceCodeViewModel? _viewModel;

    public SourceCodeView()
    {
        InitializeComponent();
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp")
                .Id));

        Editor.TextChanged += TextEditor_TextChanged;
        Editor.PointerPressed += TextEditor_PointerPressed;
        Editor.TextInput += TextEditor_TextInput;
    }

    private void TextEditor_TextInput(object? sender, TextInputEventArgs e)
    {
        UpdateCursorPosition();
    }

    private void TextEditor_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        Debug.Assert(_viewModel != null);
        try
        {
            _isInternalChange = true;
            var caretOffset = Editor.CaretOffset;
            var line = Editor.Document.GetLineByOffset(caretOffset);
            _viewModel.CurrentLine = line.LineNumber;
            _viewModel.CurrentColumn = caretOffset - line.Offset;
        }
        finally
        {
            _isInternalChange = false;
        }
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        Debug.Assert(_viewModel != null);
        try
        {
            _isInternalChange = true;
            _viewModel.SourceCode = ((TextEditor)sender!).Text;
        }
        finally
        {
            _isInternalChange = false;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        _viewModel = (SourceCodeViewModel?)DataContext;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Debug.Assert(_viewModel != null);
        if (!_isInternalChange && e.PropertyName == nameof(SourceCodeViewModel.SourceCode))
        {
            Editor.Text = _viewModel!.SourceCode;
        }

        if (e.PropertyName == nameof(SourceCodeViewModel.CurrentLine))
        {
            Editor.ScrollToLine(_viewModel!.CurrentLine);
            Editor.CaretOffset = GetCaretOffsetForLine(Editor, _viewModel.CurrentLine) + _viewModel.CurrentColumn;
        }
    }

    private static int GetCaretOffsetForLine(TextEditor textEditor, int line)
    {
        var document = textEditor.Document;
        var lineInfo = document.GetLineByNumber(line);
        return lineInfo.Offset;
    }
}