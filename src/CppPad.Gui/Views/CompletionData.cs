#region

using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using CppPad.AutoCompletion.Interface;

#endregion

namespace CppPad.Gui.Views;

public class CompletionData(AutoCompletionItem autoCompletionData) : ICompletionData
{
    public string Text { get; } = autoCompletionData.Label;

    public object Content => new TextBlock
    {
        Text = Text
    };

    public object Description => autoCompletionData.Documentation ?? autoCompletionData.Label;

    public double Priority => autoCompletionData.Priority;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        foreach (var edit in autoCompletionData.Edits)
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

    public override bool Equals(object? obj)
    {
        if (obj is CompletionData other)
        {
            return Priority.Equals(other.Priority) && string.Equals(Text, other.Text);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Priority, Text);
    }
}