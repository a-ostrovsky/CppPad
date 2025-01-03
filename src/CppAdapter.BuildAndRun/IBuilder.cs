using CppPad.BuildSystem;

namespace CppAdapter.BuildAndRun;

public interface IBuilder
{
    Task<BuildSuccessResult> BuildAsync(
        BuildConfiguration buildConfiguration,
        CancellationToken token = default
    );

    event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;
}

public class DummyBuilder : IBuilder
{
    public Task<BuildSuccessResult> BuildAsync(
        BuildConfiguration buildConfiguration,
        CancellationToken token = default
    )
    {
        BuildStatusChanged?.Invoke(
            this,
            new BuildStatusChangedEventArgs(BuildStatus.PreparingEnvironment)
        );
        return Task.FromResult(new BuildSuccessResult { CreatedFile = "C:\\x.exe" });
    }

    public event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;
}
