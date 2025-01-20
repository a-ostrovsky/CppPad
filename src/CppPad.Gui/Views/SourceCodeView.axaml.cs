#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using CppPad.Gui.AutoCompletion;
using CppPad.Gui.Input;
using CppPad.Gui.ViewModels;
using CppPad.LspClient.Model;
using TextMateSharp.Grammars;
using Range = CppPad.LspClient.Model.Range;

#endregion

namespace CppPad.Gui.Views;

public partial class SourceCodeView : UserControl
{
    private bool _isInternalChange;
    private Range? _removedRange;
    private readonly ScriptDocumentChangeListener _scriptDocumentChangeListener;

    private SourceCodeViewModel? _viewModel;

    public SourceCodeView()
    {
        InitializeComponent();
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp").Id)
        );
        
        _scriptDocumentChangeListener = new ScriptDocumentChangeListener(OnScriptDocumentUpdated);
        
        Editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        Editor.Document.Changing += TextEditor_DocumentChanging;
        Editor.Document.Changed += TextEditor_DocumentOnChanged;
        CutCommand = new RelayCommand(_ => Editor.Cut(), _ => Editor.CanCut);
        CopyCommand = new RelayCommand(_ => Editor.Copy(), _ => Editor.CanCopy);
        PasteCommand = new RelayCommand(_ => Editor.Paste(), _ => Editor.CanPaste);
    }

    private void OnScriptDocumentUpdated(IContentUpdate obj)
    {
        if (_isInternalChange)
        {
            return;
        }

        Editor.Text = obj.ScriptDocument.Script.Content;
    }

    public ICommand CutCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }

    private void TextEditor_DocumentChanging(object? sender, DocumentChangeEventArgs e)
    {
        // When removing content, then use DocumentChanging as we need original line and column information.
        if (e.RemovalLength == 0)
        {
            _removedRange = null;
            return;
        }

        var changedRange = GetChangedRangeForRemoval(e);
        _removedRange = changedRange;
    }

    private void TextEditor_DocumentOnChanged(object? sender, DocumentChangeEventArgs e)
    {
        Debug.Assert(_viewModel != null);
        try
        {
            if (e.RemovalLength != 0 && _removedRange == null)
            {
                Debug.Assert(
                    false,
                    "Document_Changed event was raised without Document_Changing event."
                );
                return;
            }

            var addedRange = e.InsertionLength > 0 ? GetChangedRangeForAdding(e) : null;

            _isInternalChange = true;
            var content = ((TextDocument)sender!).Text;
            if (_removedRange != null)
            {
                _viewModel.SetContent(
                    content,
                    _removedRange
                );
            }
            if (addedRange != null)
            {
                _viewModel.SetContent(content, addedRange, e.InsertedText.Text);
            }
        }
        finally
        {
            _isInternalChange = false;
        }
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        Debug.Assert(_viewModel != null);
        try
        {
            _isInternalChange = true;
            var (line, column) = GetLineAndColumnForCaretOffset(Editor.CaretOffset);
            _viewModel.CurrentLine = line;
            _viewModel.CurrentColumn = column;
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
            _viewModel.AddChangeListener(_scriptDocumentChangeListener);
            _viewModel.UninstallAutoCompletion();
        }

        _viewModel = (SourceCodeViewModel?)DataContext;
        if (_viewModel != null)
        {
            _viewModel.RemoveChangeListener(_scriptDocumentChangeListener);
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.InstallAutoCompletion(Editor);
            Editor.Text = _viewModel.ScriptDocument.Script.Content;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Debug.Assert(_viewModel != null);

        if (!_isInternalChange && e.PropertyName == nameof(SourceCodeViewModel.CurrentLine))
        {
            Editor.ScrollToLine(_viewModel!.CurrentLine);
            Editor.CaretOffset =
                GetCaretOffsetForLine(Editor, _viewModel.CurrentLine)
                + _viewModel.CurrentColumn
                - 1;
            Editor.TextArea.Focus();
        }

        if (!_isInternalChange && e.PropertyName == nameof(SourceCodeViewModel.CurrentColumn))
        {
            Editor.CaretOffset =
                GetCaretOffsetForLine(Editor, _viewModel.CurrentLine)
                + _viewModel.CurrentColumn
                - 1;
            Editor.TextArea.Focus();
        }
    }

    private (int, int) GetLineAndColumnForCaretOffset(int caretOffset)
    {
        var document = Editor.Document;
        var line = document.GetLineByOffset(caretOffset);
        return (line.LineNumber, caretOffset - line.Offset + 1);
    }

    private static int GetCaretOffsetForLine(TextEditor textEditor, int line)
    {
        var document = textEditor.Document;
        var lineInfo = document.GetLineByNumber(line);
        return lineInfo.Offset;
    }

    // Returns a zero based range.
    private Range GetChangedRangeForRemoval(DocumentChangeEventArgs e)
    {
        if (e.RemovalLength == 0)
        {
            throw new InvalidOperationException("This method should be called only for removal.");
        }
        
        var start = e.Offset;
        var end = e.Offset + e.RemovalLength;
        var (startLine, startColumn) = GetLineAndColumnForCaretOffset(start);
        var (endLine, endColumn) = GetLineAndColumnForCaretOffset(end);
        // In case of caret returning to the beginning of the line, the column is 0.
        var changedRange = new Range(
            new Position { Line = startLine - 1, Character = startColumn - 1 },
            new Position { Line = endLine - 1, Character = endColumn - 1 }
        );
        return changedRange;
    }
    
    private Range GetChangedRangeForAdding(DocumentChangeEventArgs e)
    {
        if (e.InsertionLength == 0)
        {
            throw new InvalidOperationException("This method should be called only for addition.");
        }
        var start = e.Offset; // The offset where the text was inserted without the length of the text.
        var (line, column) = GetLineAndColumnForCaretOffset(start);
        var changedRange = new Range(
            new Position { Line = line - 1, Character = column - 1 },
            new Position { Line = line - 1, Character = column - 1 }
        );
        return changedRange;
    }
}
