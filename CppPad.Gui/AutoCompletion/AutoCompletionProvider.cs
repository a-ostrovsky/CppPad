#region

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using CppPad.LanguageServer.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.AutoCompletion;

public class AutoCompletionProvider
{
    private readonly TextEditor _editor;
    private readonly ILanguageServer _languageServer;
    private CompletionWindow? _completionWindow;

    public AutoCompletionProvider(TextEditor editor, ILanguageServer languageServer)
    {
        _editor = editor;
        _languageServer = languageServer;
        _editor.TextArea.TextEntering += OnTextEntering;
        _editor.TextArea.KeyDown += OnKeyDown;
    }

    private void OnTextEntering(object? sender, TextInputEventArgs e)
    {
        if (!(e.Text?.Length > 0) || _completionWindow == null)
        {
            return;
        }

        if (!char.IsLetterOrDigit(e.Text[0]))
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

#pragma warning disable VSTHRD100
    private async void OnKeyDown(object? sender, KeyEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (e is not { Key: Key.Space, KeyModifiers: KeyModifiers.Control })
        {
            return;
        }

        e.Handled = true;
        await ShowCompletionWindowAsync();
    }

    private async Task ShowCompletionWindowAsync(CancellationToken token = default)
    {
        _completionWindow = new CompletionWindow(_editor.TextArea);
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

        var caretOffset = _editor.TextArea.Caret.Offset;
        var line = _editor.Document.GetLineByOffset(caretOffset);
        var lineNumber = line.LineNumber;
        var column = caretOffset - line.Offset + 1;

        var autoCompletions = await _languageServer.GetAutoCompletionAsync(
            new FileData("untitled:///untitled", _editor.Text),
            new Position(lineNumber, column),
            token);
        foreach (var autoCompletion in autoCompletions)
        {
            data.Add(new MyCompletionData(autoCompletion));
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => { _completionWindow = null; };
    }
}

public class MyCompletionData(AutoCompletionData autoCompletionData) : ICompletionData
{
    public string Text { get; } = autoCompletionData.Label;

    public object Content => new TextBlock { Text = autoCompletionData.Label };

    public object Description => string.Empty;

    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, autoCompletionData.RemainingText);
    }

    public IImage? Image => null;
}