using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidChangeConfigurationNotification
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = "workspace/didChangeConfiguration";

    [JsonPropertyName("params")]
    public DidChangeConfigurationParams Params { get; init; } = new();
}
