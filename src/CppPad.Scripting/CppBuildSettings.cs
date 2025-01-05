namespace CppPad.Scripting;

public class CppBuildSettings
{
    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public IReadOnlyList<string> LibSearchPaths { get; init; } = [];

    public IReadOnlyList<string> AdditionalEnvironmentPaths { get; init; } = [];

    public IReadOnlyList<string> LibFiles { get; init; } = [];

    public CppStandard CppStandard { get; init; } = CppStandard.Unspecified;

    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Unspecified;

    public string AdditionalBuildArgs { get; init; } = string.Empty;

    public string PreBuildCommand { get; init; } = string.Empty;
}
