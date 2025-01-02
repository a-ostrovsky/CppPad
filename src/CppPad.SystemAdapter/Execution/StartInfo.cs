namespace CppPad.SystemAdapter.Execution;

public class StartInfo
{
    public required string FileName { get; init; }

    public IList<string> Arguments { get; init; } = [];

    public IList<string> AdditionalPaths { get; init; } = [];

    public IDictionary<string, string>? EnvironmentVariables { get; init; } = null;

    public required EventHandler<DataReceivedEventArgs> OutputReceived { get; init; }

    public required EventHandler<DataReceivedEventArgs> ErrorReceived { get; init; }
}