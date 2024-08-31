using System.Text.Json;

namespace CppPad.LanguageServerAdapter.Requests;

public class CompletionRequest(string fileUri, int line, int character) : IRequest
{

    public string ToJson()
    {
        var completionRequest = new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "textDocument/completion",
            @params = new
            {
                textDocument = new { uri = fileUri },
                position = new { line = line, character = character }, // Position where you want completions
                context = new { triggerKind = 1 }
            }
        };
        var json = JsonSerializer.Serialize(completionRequest);
        return json;
    }
}