using CppPad.AutoCompletion.Clangd.Interface;
using static CppPad.AutoCompletion.Clangd.Impl.Utils;

namespace CppPad.AutoCompletion.Clangd.Impl;

public class RequestSender(ILspClient client) : IRequestSender
{
    public Task InitializeClientAsync()
    {
        return client.InitializeAsync();
    }

    public async Task<int> SendInitializeRequestAsync(int processId, string rootUri)
    {
        var requestId = client.GetNextRequestId();
        var initRequest = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "initialize",
            @params = new
            {
                processId,
                rootUri,
                capabilities = new { },
                initializationOptions = new
                {
                    fallbackFlags = new[] { "-std=c++20" }
                }
            }
        };
        await client.SendMessageAsync(initRequest);
        return requestId;
    }

    public Task SendInitializedNotificationAsync()
    {
        var initializedNotification = new
        {
            jsonrpc = "2.0",
            method = "initialized",
            @params = new { }
        };
        return client.SendMessageAsync(initializedNotification);
    }

    public Task SendDidOpenAsync(string fileName, string content)
    {
        var uri = PathToUriFormat(fileName);
        var didOpenNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didOpen",
            @params = new
            {
                textDocument = new
                {
                    uri,
                    languageId = "cpp",
                    version = 1,
                    text = content
                }
            }
        };
        return client.SendMessageAsync(didOpenNotification);
    }

    public Task SendDidChangeConfigurationAsync(IDictionary<string, object> settings)
    {
        var didChangeConfigurationNotification = new
        {
            jsonrpc = "2.0",
            method = "workspace/didChangeConfiguration",
            @params = new
            {
                settings = new
                {
                    compilationDatabaseChanges = settings
                }
            }
        };
        return client.SendMessageAsync(didChangeConfigurationNotification);
    }

    public Task SendDidChangeAsync(string fileName, int version, string text)
    {
        var uri = PathToUriFormat(fileName);
        var didChangeNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didChange",
            @params = new
            {
                textDocument = new
                {
                    uri,
                    version
                },
                contentChanges = new[]
                {
                    new { text }
                }
            }
        };
        return client.SendMessageAsync(didChangeNotification);
    }

    public Task SendDidCloseAsync(string fileName)
    {
        var uri = PathToUriFormat(fileName);
        var didCloseNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didClose",
            @params = new
            {
                textDocument = new
                {
                    uri
                }
            }
        };
        return client.SendMessageAsync(didCloseNotification);
    }

    public async Task<int> SendCompletionRequestAsync(string fileName, int line, int character)
    {
        var uri = PathToUriFormat(fileName);
        var requestId = client.GetNextRequestId();
        var completionRequest = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "textDocument/completion",
            @params = new
            {
                textDocument = new
                {
                    uri
                },
                position = new
                {
                    line,
                    character
                }
            }
        };
        await client.SendMessageAsync(completionRequest);
        return requestId;
    }
}