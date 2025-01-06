using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class TextDocumentItem
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("languageId")]
    public string LanguageId { get; init; } = "cpp";

    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
