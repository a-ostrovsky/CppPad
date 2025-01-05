using CppPad.BuildSystem;
using CppPad.SystemAdapter.Execution;

namespace CppAdapter.BuildAndRun;

public class BuildAndRunConfiguration
{
    public required BuildConfiguration BuildConfiguration { get; set; }

    public IReadOnlyList<string> ExeArguments { get; init; } = [];

    public required EventHandler<DataReceivedEventArgs> ExeOutputReceived { get; init; }

    public required EventHandler<DataReceivedEventArgs> ExeErrorReceived { get; init; }
}
