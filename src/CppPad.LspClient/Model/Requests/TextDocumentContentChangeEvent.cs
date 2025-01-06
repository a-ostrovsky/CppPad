using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class TextDocumentContentChangeEvent
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
