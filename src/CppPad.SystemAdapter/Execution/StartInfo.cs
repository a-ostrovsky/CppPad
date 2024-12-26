namespace CppPad.SystemAdapter.Execution;

public class StartInfo
{
    public required string FileName { get; init; }

    public ICollection<string> Arguments { get; init; } = [];

    public IDictionary<string, string> AdditionalPaths { get; init; } = new Dictionary<string, string>();
    
    public required EventHandler<DataReceivedEventArgs> OutputReceived { get; init; }
    
    public required EventHandler<DataReceivedEventArgs> ErrorReceived { get; init; }
}