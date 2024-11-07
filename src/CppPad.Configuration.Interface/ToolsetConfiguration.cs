namespace CppPad.Configuration.Interface;

public record ToolsetConfiguration
{
    public List<Toolset> Toolsets { get; set; } = [];

    public Guid? DefaultToolsetId { get; set; }
}