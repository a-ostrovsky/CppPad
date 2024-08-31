using CppPad.LanguageServerAdapter.Requests;
using CppPad.LanguageServerAdapter.Responses;

namespace CppPad.LanguageServerAdapter;

public class LanguageServerWorkflow(LanguageServerCommunicator communicator)
{
    public async Task<InitializeResponse> InitializeAsync(CancellationToken token = default)
    {
        var request = new InitializeRequest();
        await communicator.SendRequestAsync(request, token);
        var response = await communicator.ReadResponseAsync<InitializeResponse>(token);
        if (response == null)
        {
            throw new LanguageServerAdapterException("Failed to read initialize response");
        }
        return response;
    }

    public async Task DidOpenAsync(string fileUri, string content, int version, CancellationToken token = default)
    {
        var request = new DidOpenNotification(fileUri, content, version);
        await communicator.SendRequestAsync(request, token);
    }

    public async Task DidChangeAsync(string fileUri, string content, int version, CancellationToken token = default)
    {
        var request = new DidChangeNotification(fileUri, content, version);
        await communicator.SendRequestAsync(request, token);
    }

    public async Task<CompletionResponse> RequestCompletionAsync(
        string fileUri, int line, int character, CancellationToken token = default)
    {
        var request = new CompletionRequest(fileUri, line, character);
        await communicator.SendRequestAsync(request, token);
        var response = await communicator.ReadResponseAsync<CompletionResponse>(token);
        if (response == null)
        {
            throw new LanguageServerAdapterException("Failed to read completion response");
        }
        return response;
    }

    public async Task DidCloseAsync(string fileUri, CancellationToken token = default)
    {
        var request = new DidCloseNotification(fileUri);
        await communicator.SendRequestAsync(request, token);
    }

    public Task ShutdownAsync(CancellationToken token = default)
    {
        var request = new ShutdownRequest();
        return communicator.SendRequestAsync(request, token);
    }
}