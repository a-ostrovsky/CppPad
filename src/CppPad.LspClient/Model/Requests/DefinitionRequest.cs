using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DefinitionRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = "textDocument/definition";

    [JsonPropertyName("params")]
    public DefinitionParams Params { get; init; } = new();
}
