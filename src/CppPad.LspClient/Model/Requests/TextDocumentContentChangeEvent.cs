using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class TextDocumentContentChangeEvent
{
    [JsonPropertyName("range")]
    public Range? Range { get; init; }
    
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
