using System.Text.Json;

namespace CppPad.LanguageServerAdapter.Requests;

public class DidOpenNotification(string fileUri, string content, int version) : IRequest
{
    public string ToJson()
    {
        var didOpenNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didOpen",
            @params = new
            {
                textDocument = new
                {
                    uri = fileUri,
                    languageId = "cpp",
                    version = version,
                    text = content
                }
            }
        };
        var json = JsonSerializer.Serialize(didOpenNotification);
        return json;
    }
}