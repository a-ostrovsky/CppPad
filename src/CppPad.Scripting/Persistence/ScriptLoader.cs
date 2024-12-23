using CppPad.FileSystem;
using CppPad.Scripting.Serialization;

namespace CppPad.Scripting.Persistence;

public class ScriptLoader(ScriptSerializer serializer, DiskFileSystem fileSystem)
{
    public async Task<ScriptDocument> LoadAsync(string fileName)
    {
        var content = await fileSystem.ReadAllTextAsync(fileName);
        var result = serializer.Deserialize(content);
        return result;
    }
    
    public ScriptDocument Load(string fileName)
    {
        var content = fileSystem.ReadAllText(fileName);
        var result = serializer.Deserialize(content);
        return result;
    }
    
    public Task SaveAsync(ScriptDocument document, string fileName)
    {
        var content = serializer.Serialize(document);
        return fileSystem.WriteAllTextAsync(fileName, content);
    }
    
    public void Save(ScriptDocument document, string fileName)
    {
        var content = serializer.Serialize(document);
        fileSystem.WriteAllText(fileName, content);
    }
}