using CppPad.BuildSystem;

namespace CppAdapter.BuildAndRun;

public interface IBuildAndRunFacade
{
    Task BuildAndRunAsync(BuildAndRunConfiguration configuration, CancellationToken token = default);
    
    event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;
}

public class DummyBuildAndRunFacade : IBuildAndRunFacade
{
    public Task BuildAndRunAsync(BuildAndRunConfiguration configuration, CancellationToken token = default)
    {
        BuildStatusChanged?.Invoke(
            this,
            new BuildStatusChangedEventArgs(BuildStatus.PreparingEnvironment)
        );
        return Task.CompletedTask;
    }

    public event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;
}
