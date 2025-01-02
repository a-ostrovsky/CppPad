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
            await cmake.BuildAsync(buildConfiguration, settings, token);
        }
        finally
        {
            BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(BuildStatus.Finished));
        }
    }

    public event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;
}