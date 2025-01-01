using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.EnvironmentConfiguration;
using CppPad.Scripting.Persistence;
using CppPad.SystemAdapter.IO;

namespace CppPad.BuildSystem.CMakeAdapter;

public class CMake(
    DiskFileSystem fileSystem,
    ScriptLoader loader,
    FileBuilder fileBuilder,
    CMakeExecutor executor)
{
    public async Task BuildAsync(
        BuildConfiguration buildConfiguration,
        EnvironmentSettings environmentSettings,
        CancellationToken token = default)
    {
        var scriptDocument = buildConfiguration.ScriptDocument;
        var cppFileName = await loader.CreateCppFileAsync(scriptDocument);
        var cppFolder = Path.GetDirectoryName(cppFileName)!;
        var buildFolder = Path.Combine(cppFolder, "Build");
        await fileSystem.CreateDirectoryAsync(buildFolder);
        var options = new CMakeOptions
        {
            CppFileName = cppFileName,
            IncludeDirectories = scriptDocument.Script.BuildSettings.AdditionalIncludeDirs,
            LibFiles = scriptDocument.Script.BuildSettings.LibFiles,
            LibSearchPaths = scriptDocument.Script.BuildSettings.LibSearchPaths,
            CppStandard = scriptDocument.Script.BuildSettings.CppStandard,
            OptimizationLevel = scriptDocument.Script.BuildSettings.OptimizationLevel
        };

        var cmakeFile = fileBuilder.Build(options);
        var cmakeListsFile = Path.Combine(cppFolder, "CMakeLists.txt");
        await fileSystem.WriteAllTextAsync(cmakeListsFile, cmakeFile);
        var executionOptions = new CMakeExecutionOptions
        {
            ErrorReceived = buildConfiguration.ErrorReceived,
            ProgressReceived = buildConfiguration.ProgressReceived,
            EnvironmentSettings = environmentSettings,
            CMakeListsFolder = cppFolder,
            BuildDirectory = buildFolder,
            ForceConfigure = false
        };

        await executor.RunCMakeAsync(executionOptions, token);
    }
}