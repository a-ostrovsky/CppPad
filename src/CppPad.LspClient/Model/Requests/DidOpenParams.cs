using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidOpenParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentItem TextDocument { get; init; } = new();
}
