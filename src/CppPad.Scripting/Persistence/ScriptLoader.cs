﻿using CppPad.Scripting.Serialization;
using CppPad.SystemAdapter.IO;

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

    public Task SaveAsync(ScriptDocument script, string fileName)
    {
        var content = serializer.Serialize(script);
        return fileSystem.WriteAllTextAsync(fileName, content);
    }

    public void Save(ScriptDocument script, string fileName)
    {
        var content = serializer.Serialize(script);
        fileSystem.WriteAllText(fileName, content);
    }

    public async Task<string> CreateCppFileAsync(ScriptDocument document)
    {
        var content = document.Script.Content;
        var filePath = Path.Combine(fileSystem.SpecialFolders.TempFolder, document.Identifier.ToString(), "main.cpp");
        await fileSystem.CreateDirectoryAsync(Path.GetDirectoryName(filePath)!);
        await fileSystem.WriteAllTextAsync(filePath, content);
        return filePath;
    }
}