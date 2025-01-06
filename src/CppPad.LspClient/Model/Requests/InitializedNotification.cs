using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class InitializedNotification
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = "initialized";

    [JsonPropertyName("params")]
    public object Params { get; init; } = new();
}
