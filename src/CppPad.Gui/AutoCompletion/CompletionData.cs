using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using CppPad.LspClient.Model;

namespace CppPad.Gui.AutoCompletion;

public class CompletionData(AutoCompletionItem item) : ICompletionData
{
    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        foreach (var edit in item.Edits)
        {
            var startOffset = textArea.Document.GetOffset(edit.Range.Start.Line + 1,
                edit.Range.Start.Character + 1);
            var endOffset =
                textArea.Document.GetOffset(edit.Range.End.Line + 1, edit.Range.End.Character + 1);
            var length = endOffset - startOffset;
            textArea.Document.Replace(startOffset, length, edit.NewText);
        }
    }

    public IImage? Image => null;

    public string Text { get; } = item.Label;

    public object Content { get; } = new TextBlock
    {
        Text = item.Label
    };

    public object Description { get; } = item.Documentation ?? item.Label;

    public double Priority { get; } = item.Priority;
}