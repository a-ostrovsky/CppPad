using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class Range
{
    [JsonPropertyName("start")]
    public Position Start { get; init; } = new();

    [JsonPropertyName("end")]
    public Position End { get; init; } = new();
}