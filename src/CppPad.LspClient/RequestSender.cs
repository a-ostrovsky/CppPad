using CppPad.LspClient.Model;
using CppPad.LspClient.Model.Requests;
using static CppPad.LspClient.Utils;
using Position = CppPad.LspClient.Model.Requests.Position;
using Range = CppPad.LspClient.Model.Requests.Range;

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
            Params = new InitializeParams { ProcessId = processId, RootUri = rootUri }
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
                TextDocument = new TextDocumentItem { Uri = uri, Text = content }
            }
        };
        return proxy.SendMessageAsync(didOpenNotification);
    }

    public Task SendDidChangeConfigurationAsync(IDictionary<string, object> settings)
    {
        var didChangeConfigurationNotification = new DidChangeConfigurationNotification
        {
            Params = new DidChangeConfigurationParams
            {
                Settings = new { compilationDatabaseChanges = settings }
            }
        };
        return proxy.SendMessageAsync(didChangeConfigurationNotification);
    }

    public Task SendDidChangeAsync(string fileName, int version, IContentUpdate update)
    {
        var uri = PathToUriFormat(fileName);
        var evt = ToTextDocumentContentChangeEvent(update);
        var didChangeNotification = new DidChangeNotification
        {
            Params = new DidChangeParams
            {
                TextDocument = new VersionedTextDocumentIdentifier { Uri = uri, Version = version },
                ContentChanges =
                [
                    evt
                ]
            }
        };
        return proxy.SendMessageAsync(didChangeNotification);
    }

    private static TextDocumentContentChangeEvent ToTextDocumentContentChangeEvent(IContentUpdate update)
    {
        return update switch
        {
            FullUpdate fullUpdate => new TextDocumentContentChangeEvent
                { Text = fullUpdate.ScriptDocument.Script.Content },
            AddTextUpdate addTextUpdate => new TextDocumentContentChangeEvent
            {
                Range = new Range
                {
                    Start = new Position
                    {
                        Line = addTextUpdate.Position.Line,
                        Character = addTextUpdate.Position.Character
                    },
                    End = new Position
                    {
                        Line = addTextUpdate.Position.Line,
                        Character = addTextUpdate.Position.Character
                    }
                },
                Text = addTextUpdate.Text
            },
            RemoveTextUpdate removeTextUpdate => new TextDocumentContentChangeEvent
            {
                Range = new Range
                {
                    Start = new Position
                    {
                        Line = removeTextUpdate.Position.Line,
                        Character = removeTextUpdate.Position.Character
                    },
                    End = new Position
                    {
                        Line = removeTextUpdate.Position.Line,
                        Character = removeTextUpdate.Position.Character
                    }
                },
                Text = string.Empty
            },
            _ => throw new ArgumentException("Invalid content update type")
        };
    }

    public Task SendDidCloseAsync(string fileName)
    {
        var uri = PathToUriFormat(fileName);
        var didCloseNotification = new DidCloseNotification
        {
            Params = new DidCloseParams { TextDocument = new TextDocumentIdentifier { Uri = uri } }
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
                    Character = positionInFile.Position.Character
                }
            }
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
                    Character = positionInFile.Position.Character
                }
            }
        };
        await proxy.SendMessageAsync(definitionRequest);
        return requestId;
    }
}