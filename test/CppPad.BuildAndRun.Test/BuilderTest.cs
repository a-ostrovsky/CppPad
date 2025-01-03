using CppAdapter.BuildAndRun;
using CppPad.BuildAndRun.Test.Fakes;
using CppPad.BuildSystem;
using CppPad.BuildSystem.CMakeAdapter;
using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.MockSystemAdapter;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.BuildAndRun.Test;

public class BuilderTest
{
    [Fact]
    public async Task BuildAsync_runs_CMake()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        var process = new FakeProcess();
        var envVars = CMakeInstaller.Install(fileSystem);
        var environmentConfigurationDetector = new FakeEnvironmentConfigurationDetector
        {
            Settings = envVars
        };
        var scriptLoader = new ScriptLoader(new ScriptSerializer(), fileSystem);
        var cmake = new CMake(
            new FileWriter(scriptLoader, new FileBuilder(), fileSystem),
            new CMakeExecutor(fileSystem, process));
        var builder = new Builder(environmentConfigurationDetector, cmake);

        // Act
        await builder.BuildAsync(new BuildConfiguration
        {
            Configuration = Configuration.Debug,
            ScriptDocument = new ScriptDocument
            {
                Script = new ScriptData
                {
                    Content = "int main() { return 0; }"
                }
            },
            ErrorReceived = (_, _) => { Assert.Fail("No error expected."); },
            ProgressReceived = (_, _) => { }
        });

        // Assert
        Assert.True(process.StartCalled);
        var createdFiles = await fileSystem.ListFilesAsync(
            fileSystem.SpecialFolders.TempFolder,
            "*",
            SearchOption.AllDirectories);
        Assert.Contains(createdFiles, f => f.Contains("CMakeLists.txt"));
    }
}