using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.EnvironmentConfiguration;

namespace CppPad.BuildSystem.CMakeAdapter;

public class CMake(FileWriter cmakeFileWriter, CMakeExecutor executor)
{
    public async Task BuildAsync(
        BuildConfiguration buildConfiguration,
        EnvironmentSettings environmentSettings,
        CancellationToken token = default)
    {
        var scriptDocument = buildConfiguration.ScriptDocument;
        var fileWriterResult = await cmakeFileWriter.WriteCMakeFileAsync(scriptDocument, token);
        var executionOptions = new CMakeExecutionOptions
        {
            Configuration = buildConfiguration.Configuration,
            ErrorReceived = buildConfiguration.ErrorReceived,
            ProgressReceived = buildConfiguration.ProgressReceived,
            EnvironmentSettings = environmentSettings,
            CMakeListsFolder = fileWriterResult.CppFolder,
            BuildDirectory = fileWriterResult.BuildFolder,
            ForceConfigure = false
        };

        await executor.RunCMakeAsync(executionOptions, token);
    }
}