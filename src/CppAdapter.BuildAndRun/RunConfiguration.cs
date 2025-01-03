using CppPad.SystemAdapter.Execution;

namespace CppAdapter.BuildAndRun;

public class RunConfiguration
{
    public required string ExecutablePath { get; init; }

    public IReadOnlyList<string> Arguments { get; init; } = [];

    public required EventHandler<DataReceivedEventArgs> OutputReceived { get; init; }

    public required EventHandler<DataReceivedEventArgs> ErrorReceived { get; init; }
}