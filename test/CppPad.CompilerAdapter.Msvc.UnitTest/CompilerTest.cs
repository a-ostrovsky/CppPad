using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Impl;
using CppPad.CompilerAdapter.Msvc.UnitTest.Mocks;
using CppPad.MockFileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CppPad.CompilerAdapter.Msvc.UnitTest;

public class CompilerTest
{
    private readonly InMemoryFileSystem _fileSystem;
    private readonly CompilerProcessExecutorMock _compilerProcessExecutor;
    private readonly Compiler _compiler;

    public CompilerTest()
    {
        _fileSystem = new InMemoryFileSystem();
        var commandLineBuilder = new CommandLineBuilderMock();
        _compilerProcessExecutor = new CompilerProcessExecutorMock(_fileSystem);
        ILoggerFactory loggerFactory = new NullLoggerFactory();
        _compiler = new Compiler(_fileSystem, commandLineBuilder, _compilerProcessExecutor, loggerFactory);
    }

    [Fact]
    public async Task BuildAsync_ShouldCreateExecutable()
    {
        // Arrange
        var toolset = new Toolset("MSVC", CpuArchitecture.X86, "Microsoft Visual C++", "cl.exe");
        var buildArgs = new BuildArgs
        {
            SourceCode = "int main() { return 0; }",
            AdditionalBuildArgs = ""
        };

        // Act
        var buildTask = _compiler.BuildAsync(toolset, buildArgs);
        _compilerProcessExecutor.SignalExecutionSucceeded();
        await buildTask;

        // Assert            
        Assert.True(_fileSystem.FileExists(_compilerProcessExecutor.OutputFileName));
    }

    [Fact]
    public async Task BuildAsync_ShouldThrowCompilationFailedException_OnFailure()
    {
        // Arrange
        var toolset = new Toolset("MSVC", CpuArchitecture.X64, "Microsoft Visual C++", "cl.exe");
        var buildArgs = new BuildArgs
        {
            SourceCode = "int main() { return 0; }",
            AdditionalBuildArgs = ""
        };

        // Act & Assert
        var buildTask = _compiler.BuildAsync(toolset, buildArgs);
        _compilerProcessExecutor.SignalExecutionFailed();
        await Assert.ThrowsAsync<CompilationFailedException>(() => buildTask);
    }
}