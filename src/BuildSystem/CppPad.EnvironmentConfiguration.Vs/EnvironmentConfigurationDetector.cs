using CppPad.SystemAdapter.Execution;

namespace CppPad.EnvironmentConfiguration.Vs;

public class EnvironmentConfigurationDetector(
    DeveloperCommandPromptDetector developerCommandPromptDetector,
    Process process) : IEnvironmentConfigurationDetector
{
    public async Task<EnvironmentSettings> GetSettingsAsync(CancellationToken token = default)
    {
        var developerCommandPromptPath = await developerCommandPromptDetector.GetDeveloperCommandPromptAsync();
        var variables = await process.RunAndGetEnvironmentVariablesAsync(developerCommandPromptPath, token);
        return new EnvironmentSettings(variables);
    }
}