using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class InitializationOptions
{
    [JsonPropertyName("fallbackFlags")]
    public string[] FallbackFlags { get; init; } = ["-std=c++20"];
}
