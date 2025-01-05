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
        var scriptDocument = new ScriptDocument
        {
            Script = new ScriptData { Content = "int main() { return 0; }" },
        };
        var fileSystem = new InMemoryFileSystem();
        var scriptLoader = new ScriptLoader(new ScriptSerializer(), fileSystem);
        var cppFileName = await scriptLoader.CreateCppFileAsync(scriptDocument);
        var exeFileName = Path.ChangeExtension(Path.GetFileName(cppFileName), ".exe");
        var process = new FakeProcess();
        CreateExeFileWhenCalled(process, exeFileName, fileSystem);
        var envVars = CMakeInstaller.Install(fileSystem);
        var environmentConfigurationDetector = new FakeEnvironmentConfigurationDetector
        {
            Settings = envVars,
        };
        var cmake = new CMake(
            fileSystem,
            new FileWriter(scriptLoader, new FileBuilder(), fileSystem),
            new CMakeExecutor(fileSystem, process)
        );
        var builder = new Builder(environmentConfigurationDetector, cmake);

        // Act
        var buildResult = await builder.BuildAsync(
            new BuildConfiguration
            {
                BuildMode = BuildMode.Debug,
                ScriptDocument = scriptDocument,
                ErrorReceived = (_, _) =>
                {
                    Assert.Fail("No error expected.");
                },
                ProgressReceived = (_, _) => { },
            }
        );

        // Assert
        Assert.True(process.StartCalled);
        var createdFiles = await fileSystem.ListFilesAsync(
            fileSystem.SpecialFolders.TempFolder,
            "*",
            SearchOption.AllDirectories
        );
        Assert.Contains(createdFiles, f => f.Contains("CMakeLists.txt"));
        Assert.Equal(Path.GetFileName(buildResult.CreatedFile!), exeFileName);
    }

    private static void CreateExeFileWhenCalled(
        FakeProcess process,
        string exeFileName,
        InMemoryFileSystem fileSystem
    )
    {
        process.WhenCalledDo(args =>
        {
            if (!args.Contains("-B", StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            var buildFolder = args.SkipWhile(arg =>
                    !string.Equals(arg, "-B", StringComparison.OrdinalIgnoreCase)
                )
                .Skip(1)
                .First();
            var exeFile = Path.Combine(buildFolder, exeFileName);
            fileSystem.WriteAllText(exeFile, string.Empty);
        });
    }
}
