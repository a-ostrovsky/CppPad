namespace CppPad.Scripting.Serialization.Dtos;

public class CppBuildSettingsDto
{
    public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Unspecified;
    public CppStandard CppStandard { get; set; } = CppStandard.Unspecified;
}
