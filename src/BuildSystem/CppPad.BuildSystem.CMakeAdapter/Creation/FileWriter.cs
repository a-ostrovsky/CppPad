using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.SystemAdapter.IO;

namespace CppPad.BuildSystem.CMakeAdapter.Creation;

public class FileWriterResult
{
    public required string CppFolder { get; init; }
    public required string BuildFolder { get; init; }
}

public class FileWriter(
    ScriptLoader loader,
    FileBuilder fileBuilder,
    DiskFileSystem fileSystem)
{
    private async Task<string> CreateBuildFolderAsync(string cppFolder)
    {
        var buildFolder = Path.Combine(cppFolder, "Build");
        await fileSystem.CreateDirectoryAsync(buildFolder);
        return buildFolder;
    }

    private async Task<bool> HasCMakeFileChangedAsync(string cmakeListsFile, string newCMakeFileContent)
    {
        if (fileSystem.FileExists(cmakeListsFile))
        {
            var existingCMakeLists = await fileSystem.ReadAllTextAsync(cmakeListsFile);
            return existingCMakeLists != newCMakeFileContent;
        }

        return true;
    }

    private async Task WriteCMakeFileAsync(string cmakeListsFile, string cmakeFileContent)
    {
        await fileSystem.WriteAllTextAsync(cmakeListsFile, cmakeFileContent);
    }

    public async Task<FileWriterResult> WriteCMakeFileAsync(ScriptDocument scriptDocument,
        CancellationToken token = default)
    {
        var cppFileName = await loader.CreateCppFileAsync(scriptDocument, token);
        var cppFolder = Path.GetDirectoryName(cppFileName)!;
        token.ThrowIfCancellationRequested();
        var buildFolder = await CreateBuildFolderAsync(cppFolder);

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

        if (await HasCMakeFileChangedAsync(cmakeListsFile, cmakeFile))
        {
            await WriteCMakeFileAsync(cmakeListsFile, cmakeFile);
        }

        return new FileWriterResult
        {
            CppFolder = cppFolder,
            BuildFolder = buildFolder,
        };
    }
}