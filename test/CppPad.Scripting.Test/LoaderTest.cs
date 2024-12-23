﻿using CppPad.MockFileSystem;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;
using CppPad.UniqueIdentifier;
using DeepEqual.Syntax;

namespace CppPad.Scripting.Test;

public class LoaderTest
{
    private readonly InMemoryFileSystem _fileSystem = new();
    private readonly ScriptSerializer _serializer = new();

    [Fact]
    public async Task SaveAndLoad_ShouldReturnSameDocument()
    {
        // Arrange
        var loader = new ScriptLoader(_serializer, _fileSystem);
        var originalDocument = new ScriptDocument
        {
            Script = new ScriptData
            {
                Content = "int main() { return 0; }",
                BuildSettings = new CppBuildSettings
                {
                    OptimizationLevel = OptimizationLevel.O2,
                    CppStandard = CppStandard.Cpp17
                }
            },
            Identifier = new Identifier("12345"),
            FileName = "test.cpp"
        };
        { // Async test
            await loader.SaveAsync(originalDocument, @"c:\test.json");
            var loadedDocument = await loader.LoadAsync(@"c:\test.json");
            loadedDocument.ShouldDeepEqual(originalDocument);
        }
        {
            // Sync test
            // ReSharper disable MethodHasAsyncOverload
            loader.Save(originalDocument, @"c:\test.json");
            var loadedDocument = loader.Load(@"c:\test.json");
            // ReSharper restore MethodHasAsyncOverload
            loadedDocument.ShouldDeepEqual(originalDocument);
        }
    }
}