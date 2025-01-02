using CppPad.BuildSystem;
using CppPad.BuildSystem.CMakeAdapter;
using CppPad.EnvironmentConfiguration;

namespace CppAdapter.BuildAndRun;

public class Builder(
    IEnvironmentConfigurationDetector environmentConfigurationDetector,
    CMake cmake) : IBuilder
{
    public async Task BuildAsync(BuildConfiguration buildConfiguration, CancellationToken token = default)
    {
        try
        {
            BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(BuildStatus.PreparingEnvironment));
            var settings = await environmentConfigurationDetector.GetSettingsAsync(token);
            BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(BuildStatus.Building));
            token.ThrowIfCancellationRequested();
            await cmake.BuildAsync(buildConfiguration, settings, token);
            token.ThrowIfCancellationRequested();
            BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(BuildStatus.Succeeded));
        }
        catch (Exception e)
        {
            var status = e is OperationCanceledException ? BuildStatus.Cancelled : BuildStatus.Failed;
            BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(status));
            throw;
        }
    }

    public event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;
}