using System.Text.Json;

namespace CppPad.LanguageServerAdapter.Requests;

public class ShutdownRequest : IRequest
{
    public string ToJson()
    {
        var shutdownRequest = new
        {
            jsonrpc = "2.0",
            id = 3,
            method = "shutdown"
        };
        return JsonSerializer.Serialize(shutdownRequest);
    }
}