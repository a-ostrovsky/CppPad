using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidCloseParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; init; } = new();
}
