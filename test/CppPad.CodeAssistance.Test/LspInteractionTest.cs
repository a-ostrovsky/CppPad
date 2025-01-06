using CppPad.CodeAssistance.Test.Fakes;
using CppPad.LspClient;
using CppPad.LspClient.Model;
using CppPad.MockSystemAdapter;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;
using CppPad.UniqueIdentifier;

namespace CppPad.CodeAssistance.Test;

public class LspInteractionTest : IDisposable
{
    private readonly CodeAssistant _codeAssistant;
    private readonly FakeLspProcess _fakeLspProcess;

    public LspInteractionTest()
    {
        var fileSystem = new InMemoryFileSystem();
        _fakeLspProcess = new FakeLspProcess();
        var lspClient = new LspProxy(_fakeLspProcess);
        var requestSender = new RequestSender(lspClient);
        var responseReceiver = new ResponseReceiver(lspClient);
        var scriptLoader = new ScriptLoader(new ScriptSerializer(), fileSystem);
        _codeAssistant = new CodeAssistant(
            fileSystem,
            scriptLoader,
            responseReceiver,
            requestSender
        );
    }

    public void Dispose()
    {
        _codeAssistant.Dispose();
        _fakeLspProcess.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task OpenFileAsync_ShouldSendDidOpenNotification()
    {
        // Arrange
        var scriptDocument = new ScriptDocument
        {
            FileName = "test.cpp",
            Identifier = new Identifier("test.cpp"),
            Script = new ScriptData
            {
                Content = "int main() { return 0; }",
                BuildSettings = new CppBuildSettings { CppStandard = CppStandard.Cpp17 },
            },
        };

        // Act
        await _codeAssistant.OpenFileAsync(scriptDocument);

        // Assert
        var sentMessages = _fakeLspProcess.GetJsonObjects();
        Assert.Contains(
            sentMessages,
            message => message["method"]?.GetValue<string>() == "textDocument/didOpen"
        );
    }

    [Fact]
    public async Task InitializeAsync_ShouldInitializeClient()
    {
        // Act
        var capabilities = await _codeAssistant.RetrieveServerCapabilitiesAsync();

        // Assert
        Assert.NotEmpty(capabilities.TriggerCharacters);
    }

    [Fact]
    public async Task AutoCompletion_ShouldReturnCompletionItems()
    {
        // Arrange
        var position = new Position { Line = 0, Character = 19 };
        var scriptDocument = new ScriptDocument
        {
            FileName = "test.cpp",
            Identifier = new Identifier("test.cpp"),
            Script = new ScriptData { Content = "void X() { std::cou }" },
        };
        _fakeLspProcess.CompletionItems = ["std::cout"];

        // Act
        var completionList = await _codeAssistant.GetCompletionsAsync(scriptDocument, position);

        // Assert
        Assert.NotEmpty(completionList);
        Assert.Contains(completionList, item => item.Label == "std::cout");
    }

    [Fact]
    public async Task CloseFileAsync_ShouldSendDidCloseNotification()
    {
        // Arrange
        var scriptDocument = new ScriptDocument
        {
            FileName = "test.cpp",
            Identifier = new Identifier("test.cpp"),
            Script = new ScriptData
            {
                Content = "int main() { return 0; }",
                BuildSettings = new CppBuildSettings { CppStandard = CppStandard.Cpp17 },
            },
        };

        await _codeAssistant.OpenFileAsync(scriptDocument);

        // Act
        await _codeAssistant.CloseFileAsync(scriptDocument);

        // Assert
        var sentMessages = _fakeLspProcess.GetJsonObjects();
        Assert.Contains(
            sentMessages,
            message => message["method"]?.GetValue<string>() == "textDocument/didClose"
        );
    }

    [Fact]
    public async Task GetDefinitions_ShouldReturnDefinitionPositions()
    {
        // Arrange
        var position = new Position { Line = 0, Character = 5 };
        var scriptDocument = new ScriptDocument
        {
            FileName = "test.cpp",
            Identifier = new Identifier("test.cpp"),
            Script = new ScriptData { Content = "int main() { return 0; }" },
        };
        var expectedDefinition = new PositionInFile
        {
            FileName = "file:///test.cpp",
            Position = new Position { Line = 0, Character = 0 },
        };
        _fakeLspProcess.DefinitionPositions = [expectedDefinition];

        // Act
        var definitions = await _codeAssistant.GetDefinitionsAsync(scriptDocument, position);

        // Assert
        Assert.NotEmpty(definitions);
        Assert.Contains(
            definitions,
            def =>
                def.FileName == new Uri(expectedDefinition.FileName).LocalPath
                && def.Position.Line == expectedDefinition.Position.Line
                && def.Position.Character == expectedDefinition.Position.Character
        );
    }
}
