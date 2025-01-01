using CppPad.Scripting;

namespace CppPad.BuildSystem.CMakeAdapter.Creation;

public class CMakeOptions
{
    /// <summary>
    /// Gets or sets the path of the cpp file to compile.
    /// </summary>
    public required string CppFileName { get; init; }
    
    /// <summary>
    /// Gets or sets the paths of the include directories.
    /// </summary>
    public IReadOnlyList<string> IncludeDirectories { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the list of library files to link against.
    /// </summary>
    public IReadOnlyList<string> LibFiles { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the paths of the library search directories.
    /// </summary>
    public IReadOnlyList<string> LibSearchPaths { get; init; } = [];
    
    /// <summary>
    /// Gets or sets the C++ standard to use.
    /// </summary>
    public CppStandard CppStandard { get; init; } = CppStandard.Unspecified;
    
    /// <summary>
    /// Gets or sets the optimization level.
    /// </summary>
    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Unspecified;
    
    /// <summary>
    /// Gets or sets the compiler to use, e.g. "g++" or "clang++" or "cl.exe".
    /// </summary>
    public string? Compiler { get; init; }
}