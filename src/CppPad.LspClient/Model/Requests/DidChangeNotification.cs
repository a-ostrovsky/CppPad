using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidChangeNotification
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = "textDocument/didChange";

    [JsonPropertyName("params")]
    public DidChangeParams Params { get; init; } = new();
}
