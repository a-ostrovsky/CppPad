using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidChangeParams
{
    [JsonPropertyName("textDocument")]
    public VersionedTextDocumentIdentifier TextDocument { get; init; } = new();

    [JsonPropertyName("contentChanges")]
    public TextDocumentContentChangeEvent[] ContentChanges { get; init; } =
        [new TextDocumentContentChangeEvent()];
}
