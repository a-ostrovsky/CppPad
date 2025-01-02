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
        var settings = await environmentConfigurationDetector.GetSettingsAsync(token);
        await cmake.BuildAsync(buildConfiguration, settings, token);
    }
}