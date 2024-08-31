#region

using System.Text.Json;

#endregion

namespace CppPad.LanguageServerAdapter.Requests;

public class DidCloseNotification(string fileUri) : IRequest
{
    public string ToJson()
    {
        var didCloseNotification = new
        {
            jsonrpc = "2.0",
            method = "textDocument/didClose",
            @params = new
            {
                textDocument = new
                {
                    uri = fileUri
                }
            }
        };
        var json = JsonSerializer.Serialize(didCloseNotification);
        return json;
    }
}