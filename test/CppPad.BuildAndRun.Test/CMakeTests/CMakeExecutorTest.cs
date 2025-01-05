using CppPad.BuildSystem;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.MockSystemAdapter;

namespace CppPad.BuildAndRun.Test.CMakeTests;

public class CMakeExecutorTests
{
    [Fact]
    public async Task RunCMakeAsync_CallsConfigureAndBuild_WhenForceConfigureIsTrue()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();

        // Paths
        const string buildDir = @"C:\MyBuild";
        const string srcDir = @"C:\MyProject";
        await fileSystem.CreateDirectoryAsync(buildDir);
        await fileSystem.CreateDirectoryAsync(srcDir);
        var cmakeListsPath = Path.Combine(srcDir, "CMakeLists.txt");

        // Prepare the file system
        await fileSystem.CreateDirectoryAsync(srcDir);
        await fileSystem.WriteAllTextAsync(cmakeListsPath, "dummy cmake content");

        // Prepare the mock process
        var mockProcess = new FakeProcess
        {
            // By default, set exit code 0 (success)
            ExitCode = 0,
        };

        // Instantiate the executor
        var executor = new CMakeExecutor(fileSystem, mockProcess);

        // Prepare CMake options
        var options = new CMakeExecutionOptions
        {
            EnvironmentSettings = CMakeInstaller.Install(fileSystem),
            CMakeListsFolder = srcDir,
            BuildDirectory = buildDir,
            ForceConfigure = true,
            BuildMode = BuildMode.Debug,
        };

        // Act
        await executor.RunCMakeAsync(options);

        // Assert
        // 1. Check that the process was started (configure step)
        Assert.True(mockProcess.StartCalled, "Expected the process to start for 'configure' step.");

        // 1st is configure, 2nd is build.
        Assert.Equal(2, mockProcess.CapturedStartInfo.Count);

        // 2. Verify the first call (configure) used "cmake -S <srcDir> -B <buildDir>"
        Assert.NotNull(mockProcess.CapturedStartInfo);
        Assert.Contains("-S", mockProcess.CapturedStartInfo[0].Arguments);
        Assert.Contains(srcDir, mockProcess.CapturedStartInfo[0].Arguments);
        Assert.Contains("-B", mockProcess.CapturedStartInfo[0].Arguments);
        Assert.Contains(buildDir, mockProcess.CapturedStartInfo[0].Arguments);

        // 3. After configure, the executor calls build
        Assert.Contains("--build", mockProcess.CapturedStartInfo[1].Arguments);
        Assert.Contains(buildDir, mockProcess.CapturedStartInfo[1].Arguments);
        Assert.Contains("--config", mockProcess.CapturedStartInfo[1].Arguments);
        Assert.Contains("Debug", mockProcess.CapturedStartInfo[1].Arguments);
    }

    [Fact]
    public async Task RunCMakeAsync_SkipsConfigure_WhenCacheExistsAndNotForcing()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();

        const string buildDir = @"C:\MyBuild";
        const string srcDir = @"C:\MyProject";
        await fileSystem.CreateDirectoryAsync(buildDir);
        await fileSystem.CreateDirectoryAsync(srcDir);

        var cmakeListsPath = Path.Combine(srcDir, "CMakeLists.txt");
        var cmakeCachePath = Path.Combine(buildDir, "CMakeCache.txt");

        // Prepare file system with CMakeLists and a pre-existing CMakeCache file
        await fileSystem.CreateDirectoryAsync(srcDir);
        await fileSystem.CreateDirectoryAsync(buildDir);
        await fileSystem.WriteAllTextAsync(cmakeListsPath, "dummy cmake content");
        await fileSystem.WriteAllTextAsync(cmakeCachePath, "dummy cmake cache content");

        // Mock process
        var mockProcess = new FakeProcess { ExitCode = 0 };

        var executor = new CMakeExecutor(fileSystem, mockProcess);

        // No force re-configure
        var options = new CMakeExecutionOptions
        {
            BuildMode = BuildMode.Debug,
            EnvironmentSettings = CMakeInstaller.Install(fileSystem),
            CMakeListsFolder = srcDir,
            BuildDirectory = buildDir,
            ForceConfigure = false,
        };

        // Act
        await executor.RunCMakeAsync(options);

        // Assert
        // Because we had a CMakeCache.txt and ForceConfigure=false, we expect:
        //  - The executor to skip the configure step
        //  - Only the build step is invoked
        Assert.True(
            mockProcess.StartCalled,
            "Expected the process to be started at least once (for build)."
        );
        Assert.NotNull(mockProcess.CapturedStartInfo);

        // Because configure was skipped, the first time Start is actually called,
        // we expect build arguments:
        Assert.Contains("--build", mockProcess.CapturedStartInfo[0].Arguments);
        Assert.Contains(buildDir, mockProcess.CapturedStartInfo[0].Arguments);
    }

    [Fact]
    public async Task RunCMakeAsync_FailsIfProcessReturnsNonZero()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        fileSystem.AlwaysCreateDirectoriesIfNotExist();

        const string buildDir = @"C:\MyBuild";
        const string srcDir = @"C:\MyProject";

        await fileSystem.CreateDirectoryAsync(srcDir);
        await fileSystem.WriteAllTextAsync(
            Path.Combine(srcDir, "CMakeLists.txt"),
            "dummy cmake content"
        );

        // Mock process that returns error code
        var mockProcess = new FakeProcess { ExitCode = 1 };

        var executor = new CMakeExecutor(fileSystem, mockProcess);

        var options = new CMakeExecutionOptions
        {
            BuildMode = BuildMode.Release,
            EnvironmentSettings = CMakeInstaller.Install(fileSystem),
            CMakeListsFolder = srcDir,
            BuildDirectory = buildDir,
            ForceConfigure = true,
        };

        // Act & Assert
        await Assert.ThrowsAsync<CMakeExecutionException>(async () =>
        {
            await executor.RunCMakeAsync(options);
        });
    }

    [Fact]
    public async Task RunCMakeAsync_CapturesOutputAndError_WhenRaisedByMockProcess()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();

        const string buildDir = @"C:\BuildDir";
        const string srcDir = @"C:\SourceDir";
        await fileSystem.CreateDirectoryAsync(buildDir);
        await fileSystem.CreateDirectoryAsync(srcDir);

        await fileSystem.WriteAllTextAsync(Path.Combine(srcDir, "CMakeLists.txt"), "dummy cmake");

        var mockProcess = new FakeProcess { ExitCode = 0 };

        var progressMessages = new List<string>();
        var errorMessages = new List<string>();

        var options = new CMakeExecutionOptions
        {
            BuildMode = BuildMode.Debug,
            EnvironmentSettings = CMakeInstaller.Install(fileSystem),
            CMakeListsFolder = srcDir,
            BuildDirectory = buildDir,
            ForceConfigure = true,
            ProgressReceived = (_, e) => progressMessages.Add(e.Data),
            ErrorReceived = (_, e) => errorMessages.Add(e.Data),
        };

        var executor = new CMakeExecutor(fileSystem, mockProcess);

        // Act
        var runTask = executor.RunCMakeAsync(options);

        // Simulate the external process writing to stdout/stderr
        // (The real process might do this during configure or build steps.)
        mockProcess.RaiseOutputData("Configuring project...");
        mockProcess.RaiseOutputData("Generating build files...");
        mockProcess.RaiseErrorData("Warning: Something might be off");
        mockProcess.RaiseErrorData("Error: Some fatal error (pretend scenario)");

        await runTask;

        // Assert
        Assert.Contains("Configuring project...", progressMessages);
        Assert.Contains("Generating build files...", progressMessages);
        Assert.Contains("Warning: Something might be off", errorMessages);
        Assert.Contains("Error: Some fatal error (pretend scenario)", errorMessages);
    }
}
