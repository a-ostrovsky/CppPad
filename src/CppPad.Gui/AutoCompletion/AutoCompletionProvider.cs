#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.Gui.ErrorHandling;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.Gui.AutoCompletion;

public class AutoCompletionProvider
{
    private readonly IAutoCompletionService _autoCompletionService;

    private readonly AutoCompletionServiceUpdater _serviceUpdater;

    // Set of trigger characters. Value is not used
    private readonly ConcurrentDictionary<char, byte> _triggerCharacters = [];
    private CompletionWindow? _completionWindow;
    private TextEditor? _editor;

    public AutoCompletionProvider(IAutoCompletionService autoCompletionService, ITimer timer)
    {
        _autoCompletionService = autoCompletionService;
        _serviceUpdater = new AutoCompletionServiceUpdater(autoCompletionService, timer);
        _ = RetrieveAndSetServerCapabilitiesAsync();
    }

    private Task RetrieveAndSetServerCapabilitiesAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var capabilities = await _autoCompletionService.RetrieveServerCapabilitiesAsync();
            foreach (var c in capabilities.TriggerCharacters)
            {
                _triggerCharacters.TryAdd(c, 0);
            }
        });
    }

    public void Attach(TextEditor editor)
    {
        _editor = editor;
        _editor.KeyDown += OnKeyDown;
        _editor.TextChanged += OnEditorTextChanged;
        _editor.TextArea.TextEntered += OnTextEntered;
        _serviceUpdater.SetText(_editor.Text);
    }

    private async void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        var singleEnteredChar = e.Text is { Length: 1 } ? e.Text[0] : '\0';
        if (_triggerCharacters.ContainsKey(singleEnteredChar) && _completionWindow == null)
        {
            await ShowCompletionWindowAsync();
        }
        else if (_completionWindow != null && !char.IsControl(singleEnteredChar))
        {
            _completionWindow.Close();
            await ShowCompletionWindowAsync();
        }
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
        _editor.TextArea.TextEntered -= OnTextEntered;
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

#pragma warning disable VSTHRD100
    private async void OnKeyDown(object? sender, KeyEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (e is { Key: Key.Space, KeyModifiers: KeyModifiers.Control })
        {
            e.Handled = true;
            await ShowCompletionWindowAsync();
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

        var autoCompletions = await _autoCompletionService.GetCompletionsAsync(
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

    public Task UpdateSettingsAsync(Script script)
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(() =>
            _autoCompletionService.UpdateSettingsAsync(_serviceUpdater.FileIdentifier, script));
    }
}