#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;

#endregion

namespace CppPad.Gui.AutoCompletion;

public class AutoCompletionProvider(IAutoCompletionService autoCompletionService, ITimer timer)
{
    private readonly AutoCompletionServiceUpdater _serviceUpdater = new(autoCompletionService, timer);
    private CompletionWindow? _completionWindow;
    private TextEditor? _editor;

    public void Attach(TextEditor editor)
    {
        _editor = editor;
        _editor.KeyDown += OnKeyDown;
        _editor.TextChanged += OnEditorTextChanged;
        _serviceUpdater.SetText(_editor.Text);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        Debug.Assert(_editor != null);
        _serviceUpdater.SetText(_editor.Text);
    }

    public void Detach()
    {
        if (_editor == null)
        {
            return;
        }

        _editor.KeyDown -= OnKeyDown;
        _editor.TextChanged -= OnEditorTextChanged;
        _editor = null;
    }

    public async Task OpenNewFileAsync()
    {
        await _serviceUpdater.OpenOrRenameAsync();
        if (_editor != null)
        {
            _serviceUpdater.SetText(_editor.Text);
        }
    }

    //TODO: Use this method
    public async Task RenameFileAsync(string fileName)
    {
        await _serviceUpdater.OpenOrRenameAsync(fileName);
        if (_editor != null)
        {
            _serviceUpdater.SetText(_editor.Text);
        }
    }

#pragma warning disable VSTHRD100
    private async void OnKeyDown(object? sender, KeyEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (e is { Key: Key.Space, KeyModifiers: KeyModifiers.Control })
        {
            e.Handled = true;
            await ShowCompletionWindowAsync();
        }

        if (e.Key is Key.Enter && _completionWindow != null)
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private async Task ShowCompletionWindowAsync()
    {
        Debug.Assert(_editor != null);
        await _serviceUpdater.UpdateAsync();
        _completionWindow = new CompletionWindow(_editor.TextArea)
        {
            Width = 650
        };
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

        var caretOffset = _editor.TextArea.Caret.Offset;
        var line = _editor.Document.GetLineByOffset(caretOffset);
        var lineNumber = line.LineNumber - 1;
        var column = caretOffset - line.Offset;

        var autoCompletions = await autoCompletionService.GetCompletionsAsync(
            _serviceUpdater.FileIdentifier, lineNumber, column);
        foreach (var autoCompletion in autoCompletions)
        {
            data.Add(new CompletionData(autoCompletion));
        }

        if (_completionWindow.CompletionList.CompletionData.Count > 0)
        {
            _completionWindow.CompletionList.SelectedItem =
                _completionWindow.CompletionList.CompletionData.First();
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => { _completionWindow = null; };
    }

    
}