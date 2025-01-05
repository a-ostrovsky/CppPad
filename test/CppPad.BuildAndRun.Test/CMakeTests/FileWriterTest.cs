using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.MockSystemAdapter;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.BuildAndRun.Test.CMakeTests;

public class FileWriterTest
{
    [Fact]
    public async Task FileWriter_DoesNotChangeWriteDate_WhenTheFileIsUnchanged()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        var scriptLoader = new ScriptLoader(new ScriptSerializer(), fileSystem);
        var cmakeFileWriter = new FileWriter(scriptLoader, new FileBuilder(), fileSystem);
        var scriptDocument = new ScriptDocument
        {
            Script = new ScriptData { Content = "int main() { return 0; }" },
        };

        // Act
        await cmakeFileWriter.WriteCMakeFileAsync(scriptDocument);
        var cmakeListsFile = await GetCMakeListsFile(fileSystem);
        var firstWriteDate = fileSystem.GetLastWriteTime(cmakeListsFile);
        await cmakeFileWriter.WriteCMakeFileAsync(scriptDocument);
        var secondWriteDate = fileSystem.GetLastWriteTime(cmakeListsFile);
        Assert.Equal(firstWriteDate, secondWriteDate);
    }

    private static async Task<string> GetCMakeListsFile(InMemoryFileSystem fileSystem)
    {
        var cmakeListsFiles = await fileSystem.ListFilesAsync(
            "C:\\",
            "CMakeLists.txt",
            SearchOption.AllDirectories
        );
        Assert.Single(cmakeListsFiles);
        var cmakeListsFile = cmakeListsFiles.Single();
        return cmakeListsFile;
    }
}
