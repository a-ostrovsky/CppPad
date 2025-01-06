using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class InitializeRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = "initialize";

    [JsonPropertyName("params")]
    public InitializeParams Params { get; init; } = new();
}
