using CppPad.CompilerAdapter.Interface;

namespace CppPad.CompilerAdapter.Msvc.Interface;

public record BuildBatchFileArgs
{
    public string SourceFilePath { get; init; } = string.Empty;

    public string TargetFilePath { get; init; } = string.Empty;

    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public IReadOnlyList<string> LibrarySearchPaths { get; init; } = [];

    public IReadOnlyList<string> StaticallyLinkedLibraries { get; init; } = [];

    public string AdditionalBuildArgs { get; init; } = string.Empty;

    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Unspecified;

    public CppStandard CppStandard { get; init; } = CppStandard.Unspecified;

    public string PreBuildCommand { get; init; } = string.Empty;
}