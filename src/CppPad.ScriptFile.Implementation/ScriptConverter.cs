#region

using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.ScriptFile.Implementation;

public static class ScriptConverter
{
    public static ScriptDto ScriptToDto(Script script, Identifier? identifier = null)
    {
        return new ScriptDto
        {
            Identifier = identifier,
            Content = script.Content,
            AdditionalIncludeDirs = script.AdditionalIncludeDirs,
            LibrarySearchPaths = script.LibrarySearchPaths,
            AdditionalEnvironmentPaths = script.AdditionalEnvironmentPaths,
            StaticallyLinkedLibraries = script.StaticallyLinkedLibraries,
            CppStandard = script.CppStandard.ToString(),
            OptimizationLevel = script.OptimizationLevel.ToString(),
            AdditionalBuildArgs = script.AdditionalBuildArgs,
            PreBuildCommand = script.PreBuildCommand
        };
    }

    public static (Identifier?, Script) DtoToScript(ScriptDto scriptDto)
    {
        var identifier = scriptDto.Identifier;
        var script = new Script
        {
            Content = scriptDto.Content,
            AdditionalIncludeDirs = scriptDto.AdditionalIncludeDirs,
            LibrarySearchPaths = scriptDto.LibrarySearchPaths,
            AdditionalEnvironmentPaths = scriptDto.AdditionalEnvironmentPaths,
            StaticallyLinkedLibraries = scriptDto.StaticallyLinkedLibraries,
            CppStandard = Enum.TryParse(scriptDto.CppStandard, out CppStandard cppStandard)
                ? cppStandard
                : CppStandard.Unspecified,
            OptimizationLevel =
                Enum.TryParse(scriptDto.OptimizationLevel, out OptimizationLevel optimizationLevel)
                    ? optimizationLevel
                    : OptimizationLevel.Unspecified,
            AdditionalBuildArgs = scriptDto.AdditionalBuildArgs,
            PreBuildCommand = scriptDto.PreBuildCommand
        };
        return (identifier, script);
    }
}