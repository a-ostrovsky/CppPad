using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.EnvironmentConfiguration;
using CppPad.SystemAdapter.IO;

namespace CppPad.BuildSystem.CMakeAdapter;

public class CMake(DiskFileSystem fileSystem, FileWriter cmakeFileWriter, CMakeExecutor executor)
{
    public async Task<string?> BuildAsync(
        BuildConfiguration buildConfiguration,
        EnvironmentSettings environmentSettings,
        CancellationToken token = default
    )
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
            ForceConfigure = false,
        };

        await executor.RunCMakeAsync(executionOptions, token);

        var expectedFileName = Path.GetFileName(fileWriterResult.CppFileName);
        expectedFileName = Path.ChangeExtension(expectedFileName, ".exe");
        var createdFile = (
            await fileSystem.ListFilesAsync(
                fileWriterResult.BuildFolder,
                expectedFileName,
                SearchOption.AllDirectories
            )
        ).FirstOrDefault(f => Path.GetFileName(f) == expectedFileName);
        return createdFile;
    }
}
