namespace CppPad.SystemAdapter.Execution;

public class StartInfo
{
    public required string FileName { get; init; }

    public IList<string> Arguments { get; init; } = [];

    public IList<string> AdditionalPaths { get; init; } = [];

    public IDictionary<string, string>? EnvironmentVariables { get; init; } = null;

    public EventHandler<DataReceivedEventArgs>? OutputReceived { get; init; }

    public EventHandler<DataReceivedEventArgs>? ErrorReceived { get; init; }
    
    public bool RedirectIoStreams { get; init; }
}
