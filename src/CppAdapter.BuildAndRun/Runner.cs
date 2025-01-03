using CppPad.SystemAdapter.Execution;

namespace CppAdapter.BuildAndRun;

public class Runner(Process process) : IRunner
{
    public Task RunAsync(string executablePath, string arguments, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}