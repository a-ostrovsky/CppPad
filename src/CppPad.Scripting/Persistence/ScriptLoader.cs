using CppPad.Configuration;
using CppPad.Scripting.Serialization;
using CppPad.SystemAdapter.IO;

namespace CppPad.Scripting.Persistence;

public class ScriptLoader(ScriptSerializer serializer, DiskFileSystem fileSystem)
{
    public async Task<ScriptDocument> LoadAsync(string fileName)
    {
        var content = await fileSystem.ReadAllTextAsync(fileName);
        var result = serializer.Deserialize(content);
        result = result with { FileName = fileName };
        return result;
    }

    public async Task SaveAsync(ScriptDocument script, string fileName)
    {
        var content = serializer.Serialize(script);
        await fileSystem.WriteAllTextAsync(fileName, content);
    }

    public async Task<string> CreateCppFileAsync(
        ScriptDocument document,
        CancellationToken token = default
    )
    {
        var content = document.Script.Content;
        var filePath = Path.Combine(
            fileSystem.SpecialFolders.TempFolder,
            document.Identifier.ToString(),
            "main.cpp"
        );
        token.ThrowIfCancellationRequested();
        await fileSystem.CreateDirectoryAsync(Path.GetDirectoryName(filePath)!);
        await fileSystem.WriteAllTextAsync(filePath, content);
        return filePath;
    }
}
