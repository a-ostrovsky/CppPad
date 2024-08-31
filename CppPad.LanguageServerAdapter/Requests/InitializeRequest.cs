using System.Reflection;
using System.Text.Json;

namespace CppPad.LanguageServerAdapter.Requests;

public class InitializeRequest : IRequest
{
    private readonly Uri _rootUri;

    public InitializeRequest()
    {
        var exeFile = Assembly.GetExecutingAssembly().Location;
        var exeDirectory = Path.GetDirectoryName(exeFile) ?? string.Empty;
        var rootUri = new Uri(exeDirectory);
        _rootUri = rootUri;
    }

    public string ToJson()
    {

        var initializeRequest = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new
            {
                processId = Environment.ProcessId,
                rootUri = _rootUri,
                capabilities = new { }
            }
        };
        var json = JsonSerializer.Serialize(initializeRequest);
        return json;
    }
}