#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Impl;
using CppPad.CompilerAdapter.Msvc.Interface;
using CppPad.MockFileSystem;

#endregion

namespace CppPad.CompilerAdapter.Msvc.UnitTest;

public class CommandLineBuilderTest
{
    private readonly InMemoryFileSystem _fileSystem = new();

    [Fact]
    public void BuildBatchFile_ValidInputs_ReturnsBatchContentWithEssentialParts()
    {
        // Arrange
        _fileSystem.CreateDirectory(@"C:\path\to");
        _fileSystem.WriteAllText(@"C:\path\to\cl.exe",
            string.Empty); // Simulate the existence of cl.exe

        var toolset = new Toolset("type", CpuArchitecture.X64, "name", @"C:\path\to\cl.exe");
        var buildArgs = new BuildBatchFileArgs
        {
            SourceFilePath = "source.cpp",
            TargetFilePath = "target.exe",
            AdditionalIncludeDirs = new List<string> { "include1", "include2" },
            LibrarySearchPaths = new List<string> { @"C:\libs\path1", @"C:\libs\path2" },
            StaticallyLinkedLibraries = new List<string> { "lib1.lib", "lib2.lib" },
            AdditionalBuildArgs = "/DDEBUG",
            OptimizationLevel = OptimizationLevel.Level2,
            CppStandard = CppStandard.Cpp17,
            PreBuildCommand = "echo PreBuild"
        };

        var commandLineBuilder = new CommandLineBuilder(_fileSystem);

        // Act
        var result = commandLineBuilder.BuildBatchFile(toolset, buildArgs);

        // Assert
        var expectedVcvarsallPath = Path.GetFullPath(Path.Combine(@"C:\path\to\cl.exe",
            @"..\..\..\..\..\..\Auxiliary\Build\vcvarsall.bat"));

        Assert.Contains("echo PreBuild", result);
        Assert.Contains($"call \"{expectedVcvarsallPath}\" x64", result);
        Assert.Contains("cl.exe", result);
        Assert.Contains("source.cpp", result);
        Assert.Contains("/I\"include1\"", result);
        Assert.Contains("/I\"include2\"", result);
        Assert.Contains("/LIBPATH:\"C:\\libs\\path1\"", result);
        Assert.Contains("/LIBPATH:\"C:\\libs\\path2\"", result);
        Assert.Contains("lib1.lib", result);
        Assert.Contains("lib2.lib", result);
        Assert.Contains("/O2", result);
        Assert.Contains("/std:c++17", result);
        Assert.Contains("/DDEBUG", result);
    }

    [Fact]
    public void BuildBatchFile_CompilerDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        var toolset = new Toolset("type", CpuArchitecture.X64, "name", "path/to/cl.exe");
        var buildArgs = new BuildBatchFileArgs
        {
            SourceFilePath = "source.cpp",
            TargetFilePath = "target.exe",
            AdditionalIncludeDirs = new List<string>(),
            AdditionalBuildArgs = "",
            OptimizationLevel = OptimizationLevel.Level0,
            CppStandard = CppStandard.Cpp11,
            PreBuildCommand = ""
        };

        var commandLineBuilder = new CommandLineBuilder(fileSystem);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => commandLineBuilder.BuildBatchFile(toolset, buildArgs));
    }
}