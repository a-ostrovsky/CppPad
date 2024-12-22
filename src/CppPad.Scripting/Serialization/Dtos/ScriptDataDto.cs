namespace CppPad.Scripting.Serialization.Dtos;

public class ScriptDataDto
{
    public string Content { get; set; } = string.Empty;
    public CppBuildSettingsDto BuildSettings { get; set; } = new();
}