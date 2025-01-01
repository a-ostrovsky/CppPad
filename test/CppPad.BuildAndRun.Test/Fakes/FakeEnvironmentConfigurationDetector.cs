using CppPad.EnvironmentConfiguration;

namespace CppPad.BuildAndRun.Test.Fakes;

public class FakeEnvironmentConfigurationDetector : IEnvironmentConfigurationDetector
{
    public EnvironmentSettings Settings { get; set; } = new();
    
    public Task<EnvironmentSettings> GetSettingsAsync(CancellationToken token = default)
    {
        return Task.FromResult(Settings);
    }
}