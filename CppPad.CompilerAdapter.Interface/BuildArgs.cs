namespace CppPad.CompilerAdapter.Interface;

public record BuildArgs
{
    public string SourceCode { get; init; } = string.Empty;

    public string PreBuildCommand { get; init; } = string.Empty;

    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public CppStandard CppStandard { get; init; } = CppStandard.CppLatest;

    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Unspecified;

    public string AdditionalBuildArgs { get; init; } = string.Empty;
}