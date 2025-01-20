using CppPad.BuildSystem;
using CppPad.BuildSystem.CMakeAdapter;
using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.MockSystemAdapter;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.BuildAndRun.Test.CMakeTests;

public class CMakeTest
{
    [Fact]
    public async Task BuildAsync_CreatesDirectoriesAndRunsCMake()
    {
        // Arrange
        var process = new FakeProcess();
        var fileSystem = new InMemoryFileSystem();
        var loader = new ScriptLoader(new ScriptSerializer(), fileSystem);
        var fileBuilder = new FileBuilder();
        var fileWriter = new FileWriter(loader, fileBuilder, fileSystem);
        var executor = new CMakeExecutor(fileSystem, process);
        var cmake = new CMake(fileSystem, fileWriter, executor);

        var scriptDocument = new ScriptDocument
        {
            Script = new ScriptData
            {
                BuildSettings = new CppBuildSettings
                {
                    AdditionalIncludeDirs = new List<string> { "include/dir1" },
                    LibSearchPaths = new List<string> { "lib/path1", "lib/path2" },
                    AdditionalEnvironmentPaths = new List<string> { "env/path1" },
                    LibFiles = new List<string> { "libfile1.lib" },
                    CppStandard = CppStandard.Cpp17,
                    OptimizationLevel = OptimizationLevel.O2,
                },
                Content = "int main() { return 0; }"
            }
        };

        // Act
        await cmake.BuildAsync(
            new BuildConfiguration
            {
                BuildMode = BuildMode.Debug,
                ScriptDocument = scriptDocument,
                ErrorReceived = (_, _) => { Assert.Fail("No error expected."); },
                ProgressReceived = (_, _) => { }
            },
            CMakeInstaller.Install(fileSystem)
        );

        // Assert
        Assert.True(process.StartCalled);
        var createdFiles = await fileSystem.ListFilesAsync(
            fileSystem.SpecialFolders.TempFolder,
            "*",
            SearchOption.AllDirectories
        );
        Assert.Contains(createdFiles, f => f.Contains("CMakeLists.txt"));
        var cmakeListsFile = createdFiles.First(f => f.Contains("CMakeLists.txt"));
        var cmakeContent = await fileSystem.ReadAllTextAsync(cmakeListsFile);
        Assert.Contains("set(CMAKE_CXX_STANDARD 17)", cmakeContent);
        Assert.Contains("set(CMAKE_CXX_FLAGS \"${CMAKE_CXX_FLAGS} -O2\")", cmakeContent);
        Assert.Contains("include_directories(include/dir1)", cmakeContent);
        Assert.Contains("link_directories(lib/path1)", cmakeContent);
        Assert.Contains("target_link_libraries(main libfile1.lib)", cmakeContent);
    }
}