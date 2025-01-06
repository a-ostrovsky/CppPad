using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidCloseNotification
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = "textDocument/didClose";

    [JsonPropertyName("params")]
    public DidCloseParams Params { get; init; } = new();
}
