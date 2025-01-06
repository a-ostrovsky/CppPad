using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class InitializeParams
{
    [JsonPropertyName("processId")]
    public int ProcessId { get; init; }

    [JsonPropertyName("rootUri")]
    public string RootUri { get; init; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public string[] Capabilities { get; init; } = [];

    [JsonPropertyName("initializationOptions")]
    public InitializationOptions InitializationOptions { get; init; } = new();
}
