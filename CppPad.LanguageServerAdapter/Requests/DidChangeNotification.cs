using System.Text.Json;

namespace CppPad.LanguageServerAdapter.Requests;

public class DidChangeNotification(string fileUri, string content, int version) : IRequest
{
    public string ToJson()
    {
        var didChangeNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didChange",
            @params = new
            {
                TextDocument = new
                {
                    Uri = fileUri,
                    Version = version
                },
                ContentChanges = new[]
                {
                    new
                    {
                        Text = content
                    }
                }
            }
        };
        var json = JsonSerializer.Serialize(didChangeNotification);
        return json;
    }
}