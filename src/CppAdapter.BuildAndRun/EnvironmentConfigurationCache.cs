using CppPad.Common;
using CppPad.EnvironmentConfiguration;

namespace CppAdapter.BuildAndRun;

public class EnvironmentConfigurationCache : IEnvironmentConfigurationDetector
{
    public async Task<EnvironmentSettings> GetSettingsAsync(CancellationToken token = default)
    {
        using var lck = await _lock.LockAsync();
        return _settings ??= await _detector.GetSettingsAsync(token);
    }

    private readonly AsyncLock _lock = new();
    private EnvironmentSettings? _settings;
    private readonly IEnvironmentConfigurationDetector _detector;

    public EnvironmentConfigurationCache(IEnvironmentConfigurationDetector detector)
    {
        _detector = detector;
        _ = GetSettingsAsync(); // Preload settings
    }
}
