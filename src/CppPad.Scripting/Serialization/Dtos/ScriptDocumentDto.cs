namespace CppPad.Scripting.Serialization.Dtos;

public class ScriptDocumentDto
{
    public ScriptDataDto Script { get; set; } = new();
    public string Identifier { get; set; } = string.Empty;
    public string? FileName { get; set; }
}