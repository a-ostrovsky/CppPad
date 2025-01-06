using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class TextDocumentIdentifier
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;
}
