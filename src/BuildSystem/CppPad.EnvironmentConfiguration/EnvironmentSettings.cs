namespace CppPad.EnvironmentConfiguration;

public class EnvironmentSettings
{
    private readonly Dictionary<string, string> _environmentVariables = new(StringComparer.OrdinalIgnoreCase);

    public EnvironmentSettings(IDictionary<string, string> environmentVariables)
    {
        foreach (var (key, value) in environmentVariables)
        {
            Add(key, value);
        }
    }

    public EnvironmentSettings()
    {
    }

    public void Add(string key, string value)
    {
        if (!_environmentVariables.TryAdd(key, value))
        {
            throw new ArgumentException($"Key {key} already exists.", nameof(key));
        }
    }

    public string? TryGet(string key)
    {
        return _environmentVariables.GetValueOrDefault(key);
    }
}