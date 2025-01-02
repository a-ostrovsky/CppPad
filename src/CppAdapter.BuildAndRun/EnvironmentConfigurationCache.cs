using CppPad.Common;
using CppPad.EnvironmentConfiguration;

namespace CppAdapter.BuildAndRun;

public class EnvironmentConfigurationCache(IEnvironmentConfigurationDetector detector)
    : IEnvironmentConfigurationDetector
{
    public async Task<EnvironmentSettings> GetSettingsAsync(CancellationToken token = default)
    {
        using var lck = await _lock.LockAsync();
        return _settings ??= await detector.GetSettingsAsync(token);
    }
    
    private readonly AsyncLock _lock = new();
    private EnvironmentSettings? _settings;
}