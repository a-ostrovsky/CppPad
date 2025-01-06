using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class Position
{
    [JsonPropertyName("line")]
    public int Line { get; init; }

    [JsonPropertyName("character")]
    public int Character { get; init; }
}
