namespace CppPad.LanguageServerAdapter.Responses;

public class CompletionResponse
{
    public string Jsonrpc { get; init; } = string.Empty;
    public int Id { get; init; }
    public CompletionResult Result { get; init; } = new();
}

public class CompletionResult
{
    public bool IsIncomplete { get; init; }
    public CompletionItem[] Items { get; init; } = [];
}

public class CompletionItem
{
    public string Label { get; init; } = string.Empty;
    public int Kind { get; init; }
    public string Detail { get; init; } = string.Empty;
    public Documentation Documentation { get; init; } = new();
    public string SortText { get; init; } = string.Empty;
    public string FilterText { get; init; } = string.Empty;
    public string InsertText { get; init; } = string.Empty;
    public int InsertTextFormat { get; init; }
}

public class Documentation
{
    public string Kind { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
