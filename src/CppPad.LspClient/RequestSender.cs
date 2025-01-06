using CppPad.LspClient.Model;
using CppPad.LspClient.Model.Requests;
using static CppPad.LspClient.Utils;
using Position = CppPad.LspClient.Model.Requests.Position;

namespace CppPad.LspClient;

public class RequestSender(LspProxy proxy)
{
    public Task InitializeClientAsync()
    {
        return proxy.InitializeAsync();
    }

    public async Task<int> SendInitializeRequestAsync(int processId, string rootUri)
    {
        var requestId = proxy.GetNextRequestId();
        var initRequest = new InitializeRequest
        {
            Id = requestId,
            Params = new InitializeParams { ProcessId = processId, RootUri = rootUri },
        };
        await proxy.SendMessageAsync(initRequest);
        return requestId;
    }

    public Task SendInitializedNotificationAsync()
    {
        var initializedNotification = new InitializedNotification();
        return proxy.SendMessageAsync(initializedNotification);
    }

    public Task SendDidOpenAsync(string fileName, string content)
    {
        var uri = PathToUriFormat(fileName);
        var didOpenNotification = new DidOpenNotification
        {
            Params = new DidOpenParams
            {
                TextDocument = new TextDocumentItem { Uri = uri, Text = content },
            },
        };
        return proxy.SendMessageAsync(didOpenNotification);
    }

    public Task SendDidChangeConfigurationAsync(IDictionary<string, object> settings)
    {
        var didChangeConfigurationNotification = new DidChangeConfigurationNotification
        {
            Params = new DidChangeConfigurationParams
            {
                Settings = new { compilationDatabaseChanges = settings },
            },
        };
        return proxy.SendMessageAsync(didChangeConfigurationNotification);
    }

    public Task SendDidChangeAsync(string fileName, int version, string text)
    {
        var uri = PathToUriFormat(fileName);
        var didChangeNotification = new DidChangeNotification
        {
            Params = new DidChangeParams
            {
                TextDocument = new VersionedTextDocumentIdentifier { Uri = uri, Version = version },
                ContentChanges = [new TextDocumentContentChangeEvent { Text = text }],
            },
        };
        return proxy.SendMessageAsync(didChangeNotification);
    }

    public Task SendDidCloseAsync(string fileName)
    {
        var uri = PathToUriFormat(fileName);
        var didCloseNotification = new DidCloseNotification
        {
            Params = new DidCloseParams { TextDocument = new TextDocumentIdentifier { Uri = uri } },
        };
        return proxy.SendMessageAsync(didCloseNotification);
    }

    public async Task<int> SendCompletionRequestAsync(PositionInFile positionInFile)
    {
        var uri = PathToUriFormat(positionInFile.FileName);
        var requestId = proxy.GetNextRequestId();
        var completionRequest = new CompletionRequest
        {
            Id = requestId,
            Params = new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position
                {
                    Line = positionInFile.Position.Line,
                    Character = positionInFile.Position.Character,
                },
            },
        };
        await proxy.SendMessageAsync(completionRequest);
        return requestId;
    }

    public async Task<int> SendFindDefinitionAsync(PositionInFile positionInFile)
    {
        var uri = PathToUriFormat(positionInFile.FileName);
        var requestId = proxy.GetNextRequestId();
        var definitionRequest = new DefinitionRequest
        {
            Id = requestId,
            Params = new DefinitionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position
                {
                    Line = positionInFile.Position.Line,
                    Character = positionInFile.Position.Character,
                },
            },
        };
        await proxy.SendMessageAsync(definitionRequest);
        return requestId;
    }
}
