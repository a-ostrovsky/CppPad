﻿#region

using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.MockFileSystem;
using CppPad.ScriptFile.Implementation;
using CppPad.ScriptFile.Interface;
using DeepEqual.Syntax;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.ScriptFile.UnitTest;

public class ScriptLoaderTest
{
    private readonly InMemoryFileSystem _fileSystem = new();
    private readonly ScriptLoader _scriptLoader;

    public ScriptLoaderTest()
    {
        _scriptLoader = new ScriptLoader(_fileSystem, new ScriptParser(NullLoggerFactory.Instance),
            NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task LoadAsync_CppFile_FileExists_ReturnsScript()
    {
        // Arrange
        const string path = @"C:\test.cpp";
        const string content = "int main() { return 0; }";
        await _fileSystem.WriteAllTextAsync(path, content);

        // Act
        var scriptDocument = await _scriptLoader.LoadAsync(path);

        // Assert
        Assert.NotNull(scriptDocument);
        Assert.Equal(content, scriptDocument.Script.Content);
    }

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        const string path = @"C:\nonexistent.cpp";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _scriptLoader.LoadAsync(path));
    }

    [Fact]
    public async Task SaveAsyncAndLoadAsync_ValidScript_CanLoadSavedScript()
    {
        // Arrange
        const string path = @"C:\test" + AppConstants.DefaultFileExtension;
        var script = new Script
        {
            Content = "int main() { return 0; }",
            AdditionalIncludeDirs = ["X:\\Includes"],
            LibrarySearchPaths = ["X:\\Libs"],
            AdditionalEnvironmentPaths = ["X:\\Env"],
            StaticallyLinkedLibraries = ["lib1", "lib2"],
            CppStandard = CppStandard.Cpp11,
            OptimizationLevel = OptimizationLevel.Level1,
            AdditionalBuildArgs = "Args1",
            PreBuildCommand = "PreBuildCommand1"
        };

        var scriptDocument = new ScriptDocument
        {
            Identifier = IdGenerator.GenerateUniqueId(),
            FileName = path,
            Script = script
        };

        // Act
        await _scriptLoader.SaveAsync(scriptDocument);
        var loadedScript = await _scriptLoader.LoadAsync(path);

        // Assert
        loadedScript.ShouldDeepEqual(scriptDocument);
    }
}