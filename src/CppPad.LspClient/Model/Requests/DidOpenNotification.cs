using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidOpenNotification
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = "textDocument/didOpen";

    [JsonPropertyName("params")]
    public DidOpenParams Params { get; init; } = new();
}
