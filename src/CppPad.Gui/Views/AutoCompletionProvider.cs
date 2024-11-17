#region

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using CppPad.AutoCompletion.Interface;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.ViewModels;

#endregion

namespace CppPad.Gui.Views;

public class AutoCompletionProvider
{
    private readonly EditorViewModel _editorViewModel;

    // Set of trigger characters. Value is not used
    private readonly ConcurrentDictionary<char, byte> _triggerCharacters = [];
    private CompletionWindow? _completionWindow;
    private TextEditor? _editor;

    public AutoCompletionProvider(EditorViewModel editorViewModel)
    {
        _editorViewModel = editorViewModel;
        _ = RetrieveAndSetServerCapabilitiesAsync();
    }

    private Task RetrieveAndSetServerCapabilitiesAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var capabilities =
                await _editorViewModel.AutoCompletionService.RetrieveServerCapabilitiesAsync();
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
        _editor.TextArea.TextEntered += OnTextEntered;
    }

    // ReSharper disable once AsyncVoidMethod : RunWithErrorHandlingAsync is not throwing
    private async void OnTextEntered(object? sender, TextInputEventArgs e)
    {
        await ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var singleEnteredChar = e.Text is { Length: 1 } ? e.Text[0] : '\0';
            if (_triggerCharacters.ContainsKey(singleEnteredChar))
            {
                _completionWindow?.Close();
                await ShowCompletionWindowAsync();
            }
            else if (_completionWindow != null && !char.IsControl(singleEnteredChar))
            {
                await UpdateCompletionWindowAsync(_completionWindow);
            }
        });
    }

    public void Detach()
    {
        if (_editor == null)
        {
            return;
        }

        _editor.KeyDown -= OnKeyDown;
        _editor.TextArea.TextEntered -= OnTextEntered;
        _editor = null;
    }

    // ReSharper disable once AsyncVoidMethod : RunWithErrorHandlingAsync is not throwing
    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        await ErrorHandler.Instance.RunWithErrorHandlingAsync(() =>
        {
            if (e is { Key: Key.Space, KeyModifiers: KeyModifiers.Control })
            {
                e.Handled = true;
                return ShowCompletionWindowAsync();
            }

            return Task.CompletedTask;
        });
    }

    public async Task ShowDefinitionsAsync()
    {
        var scriptDocument = _editorViewModel.GetCurrentScriptDocument();
        await _editorViewModel.AutoCompletionService.UpdateContentAsync(scriptDocument);

        Debug.Assert(_editor != null);
        var caretOffset = _editor.TextArea.Caret.Offset;
        var line = _editor.TextArea.Document.GetLineByOffset(caretOffset);
        var lineNumber = line.LineNumber - 1;
        var column = caretOffset - line.Offset;
        var position = new Position { Line = lineNumber, Character = column };

        var definitions = await _editorViewModel.AutoCompletionService
            .GetDefinitionsAsync(scriptDocument, position);
        await _editorViewModel.GoToDefinitionsAsync(definitions);
    }

    private async Task ShowCompletionWindowAsync()
    {
        Debug.Assert(_editor != null);
        _completionWindow = new CompletionWindow(_editor.TextArea)
        {
            Width = 650
        };
        await UpdateCompletionWindowAsync(_completionWindow);

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => { _completionWindow = null; };
    }

    private async Task UpdateCompletionWindowAsync(CompletionWindow completionWindow)
    {
        var scriptDocument = _editorViewModel.GetCurrentScriptDocument();
        await _editorViewModel.AutoCompletionService.UpdateContentAsync(scriptDocument);

        var caretOffset = completionWindow.TextArea.Caret.Offset;
        var line = completionWindow.TextArea.Document.GetLineByOffset(caretOffset);
        var lineNumber = line.LineNumber - 1;
        var column = caretOffset - line.Offset;
        var position = new Position { Line = lineNumber, Character = column };

        var data = completionWindow.CompletionList.CompletionData;
        completionWindow.CompletionList.IsFiltering = false;

        // Build sets of existing and new items
        var autoCompletions = await _editorViewModel.AutoCompletionService
            .GetCompletionsAsync(scriptDocument, position);
        data.Clear();
        foreach (var completion in autoCompletions)
        {
            data.Add(new CompletionData(completion));
        }

        // Preserve the selected item if it still exists
        var selectedItem = completionWindow.CompletionList.SelectedItem;
        if (selectedItem != null && data.Contains(selectedItem))
        {
            completionWindow.CompletionList.SelectedItem = selectedItem;
        }
        else if (data.Count > 0)
        {
            completionWindow.CompletionList.SelectedItem = data.First();
        }
    }
}