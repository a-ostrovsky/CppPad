namespace CppPad.EnvironmentConfiguration;

public interface IEnvironmentConfigurationDetector
{
    Task<EnvironmentSettings> GetSettingsAsync(CancellationToken token = default);
}