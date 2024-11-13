#region

using CppPad.AutoCompletion.Clangd.Impl;
using CppPad.AutoCompletion.Clangd.UnitTest.Mocks;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.AutoCompletion.Clangd.UnitTest;

public class InitializationTest : IDisposable
{
    private readonly ClangdProcessProxyMock _clangdProcessProxyMock;
    private readonly ClangdService _clangdService;

    public InitializationTest()
    {
        _clangdProcessProxyMock = new ClangdProcessProxyMock();
        var lspClient = new LspClient(_clangdProcessProxyMock, new NullLoggerFactory());
        var requestSender = new RequestSender(lspClient);
        var responseReceiver = new ResponseReceiver(lspClient);
        var scriptLoader = new ScriptLoaderMock();

        _clangdService = new ClangdService(scriptLoader, responseReceiver, requestSender);
    }
    
    public void Dispose()
    {
        _clangdService.Dispose();
        _clangdProcessProxyMock.Kill();
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
            Script = new Script
            {
                Content = "int main() { return 0; }",
                CppStandard = CppStandard.Cpp17
            }
        };

        // Act
        await _clangdService.OpenFileAsync(scriptDocument);

        // Assert
        var sentMessages = _clangdProcessProxyMock.GetJsonObjects();
        Assert.Contains(sentMessages, message =>
            message["method"]?.GetValue<string>() == "textDocument/didOpen");
    }

    [Fact]
    public async Task InitializeAsync_ShouldInitializeClient()
    {
        // Act
        var capabilities = await _clangdService.RetrieveServerCapabilitiesAsync();

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
            Script = new Script
            {
                Content = "void X() { std::cou }"
            }
        };
        _clangdProcessProxyMock.CompletionItems = ["std::cout"];
        
        // Act
        var completionList = await _clangdService.GetCompletionsAsync(scriptDocument, position);

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
            Script = new Script
            {
                Content = "int main() { return 0; }",
                CppStandard = CppStandard.Cpp17
            }
        };

        await _clangdService.OpenFileAsync(scriptDocument);

        // Act
        await _clangdService.CloseFileAsync(scriptDocument);

        // Assert
        var sentMessages = _clangdProcessProxyMock.GetJsonObjects();
        Assert.Contains(sentMessages, message =>
            message["method"]?.GetValue<string>() == "textDocument/didClose");
    }

}