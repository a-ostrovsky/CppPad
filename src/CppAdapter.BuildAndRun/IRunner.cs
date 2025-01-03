namespace CppAdapter.BuildAndRun;

public interface IRunner
{
    Task RunAsync(RunConfiguration runConfiguration, CancellationToken cancellationToken);
}

public class DummyRunner : IRunner
{
    public Task RunAsync(RunConfiguration runConfiguration, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
