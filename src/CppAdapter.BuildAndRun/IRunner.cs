namespace CppAdapter.BuildAndRun;

public interface IRunner
{
    Task RunAsync(string executablePath, string arguments, CancellationToken cancellationToken);
}

public class DummyRunner : IRunner
{
    public Task RunAsync(string executablePath, string arguments, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}