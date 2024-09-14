#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Interface;
using CppPad.FileSystem;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Impl;

public class CommandLineBuilder(DiskFileSystem fileSystem) : ICommandLineBuilder
{
    public string BuildBatchFile(Toolset toolset, BuildBatchFileArgs buildArgs)
    {
        var clExePath = toolset.ExecutablePath;
        if (!fileSystem.FileExists(clExePath))
        {
            throw new ArgumentException($"File '{clExePath}' does not exist.");
        }

        // Determine the path to vcvarsall.bat based on the cl.exe path
        var clExeDirectory = Path.GetDirectoryName(clExePath)!;
        var vcvarsallPath = Path.Combine(
            clExeDirectory, @"..\..\..\..\..\..\Auxiliary\Build\vcvarsall.bat");
        vcvarsallPath = Path.GetFullPath(vcvarsallPath);

        var optimizationLevel = buildArgs.OptimizationLevel switch
        {
            OptimizationLevel.Level0 => "/Od",
            OptimizationLevel.Level1 => "/O1",
            OptimizationLevel.Level2 => "/O2",
            OptimizationLevel.Level3 => "/Ox",
            _ => string.Empty
        };

        var cppStandard = buildArgs.CppStandard switch
        {
            CppStandard.Cpp11 => "/std:c++11",
            CppStandard.Cpp14 => "/std:c++14",
            CppStandard.Cpp17 => "/std:c++17",
            CppStandard.Cpp20 => "/std:c++20",
            CppStandard.Cpp23 => "/std:c++23",
            CppStandard.CppLatest => "/std:c++latest",
            _ => string.Empty
        };

        var includeDirs = string.Join(" ",
            buildArgs.AdditionalIncludeDirs.Select(dir => $"/I\"{dir}\""));
        var libraryPaths = string.Join(" ",
            buildArgs.LibrarySearchPaths.Select(path => $"/LIBPATH:\"{path}\""));
        var libraries = string.Join(" ", buildArgs.StaticallyLinkedLibraries);

        var architecture = toolset.TargetArchitecture switch
        {
            CpuArchitecture.X64 => "x64",
            CpuArchitecture.X86 => "x86",
            _ => throw new ArgumentException(
                $"Unsupported architecture: {toolset.TargetArchitecture}")
        };

        var clExeCall =
            $@"cl.exe /Fe""{buildArgs.TargetFilePath}"" {buildArgs.SourceFilePath} {includeDirs} "
            + $"{optimizationLevel} {cppStandard} {buildArgs.AdditionalBuildArgs} /link {libraryPaths} {libraries}";

        var batchContent =
            $"""
              @echo off
              {buildArgs.PreBuildCommand}
              call "{vcvarsallPath}" {architecture}
              {clExeCall}
             """;
        return batchContent;
    }
}