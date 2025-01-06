namespace CppPad.LspClient.Model;

public record AutoCompletionItem
{
    public string Label { get; init; } = string.Empty;

    public double Priority { get; init; }

    public string? Documentation { get; init; }

    public IReadOnlyList<Edit> Edits { get; init; } = [];
}
