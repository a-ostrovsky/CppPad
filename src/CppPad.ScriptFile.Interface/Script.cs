using CppPad.CompilerAdapter.Interface;

namespace CppPad.ScriptFile.Interface;

public record Script
{
    public string Content { get; init; } = string.Empty;

    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public IReadOnlyList<string> LibrarySearchPaths { get; init; } = [];

    public IReadOnlyList<string> AdditionalEnvironmentPaths { get; init; } = [];

    public IReadOnlyList<string> StaticallyLinkedLibraries { get; init; } = [];

    public CppStandard CppStandard { get; init; } = CppStandard.CppLatest;

    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Unspecified;

    public string AdditionalBuildArgs { get; init; } = string.Empty;

    public string PreBuildCommand { get; init; } = string.Empty;
}