using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class VersionedTextDocumentIdentifier
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public int Version { get; init; }
}
