using CppPad.SystemAdapter.Execution;

namespace CppAdapter.BuildAndRun;

public class Runner(Process process) : IRunner
{
    public Task RunAsync(RunConfiguration runConfiguration, CancellationToken cancellationToken)
    {
        var info = process.Start(
            new StartInfo
            {
                FileName = runConfiguration.ExecutablePath,
                Arguments = [.. runConfiguration.Arguments],
                OutputReceived = runConfiguration.OutputReceived,
                ErrorReceived = runConfiguration.ErrorReceived,
            }
        );
        return process.WaitForExitAsync(info, cancellationToken);
    }
}
