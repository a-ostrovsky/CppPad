using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DidChangeConfigurationParams
{
    [JsonPropertyName("settings")]
    public object Settings { get; init; } = new();
}
