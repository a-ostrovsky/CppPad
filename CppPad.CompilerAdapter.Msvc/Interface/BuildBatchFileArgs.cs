namespace CppPad.CompilerAdapter.Msvc.Interface;

public record BuildBatchFileArgs
{
    public string SourceFilePath { get; init; } = string.Empty;

    public string TargetFilePath { get; init; } = string.Empty;

    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public string AdditionalBuildArgs { get; init; } = string.Empty;
}