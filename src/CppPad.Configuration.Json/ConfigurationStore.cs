#region

using System.Text.Json;
using CppPad.Configuration.Interface;
using CppPad.FileSystem;

#endregion

namespace CppPad.Configuration.Json;

public class ConfigurationStore(
    SettingsFile settingsFile,
    DiskFileSystem fileSystem) : IConfigurationStore
{
    private static readonly JsonSerializerOptions
        JsonSerializerOptions = new() { WriteIndented = true };

    public async Task SaveToolsetConfigurationAsync(
        ToolsetConfiguration toolsetConfiguration)
    {
        var config = await LoadConfigurationAsync();
        config.ToolsetConfiguration = toolsetConfiguration;
        var toolsetsJson = JsonSerializer.Serialize(
            config,
            JsonSerializerOptions
        );
        await fileSystem.WriteAllTextAsync(settingsFile.GetOrCreateFile(), toolsetsJson);
    }

    public async Task<IReadOnlyList<string>> GetLastOpenedFileNamesAsync()
    {
        var config = await LoadConfigurationAsync();
        return config.RecentFiles;
    }

    public async Task SaveLastOpenedFileNameAsync(string fileName)
    {
        var config = await LoadConfigurationAsync();
        // The number of files is small, so this O(N) operation is acceptable.
        config.RecentFiles.Remove(fileName);
        config.RecentFiles.Insert(0, fileName);
        if (config.RecentFiles.Count > IConfigurationStore.MaxRecentFiles)
        {
            config.RecentFiles.RemoveAt(IConfigurationStore.MaxRecentFiles);
        }

        var configJson = JsonSerializer.Serialize(config, JsonSerializerOptions);
        await fileSystem.WriteAllTextAsync(settingsFile.GetOrCreateFile(), configJson);
        LastOpenedFileNamesChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? LastOpenedFileNamesChanged;

    public async Task<ToolsetConfiguration> GetToolsetConfigurationAsync()
    {
        return (await LoadConfigurationAsync()).ToolsetConfiguration;
    }

    public ToolsetConfiguration GetToolsetConfiguration()
    {
        return LoadConfiguration().ToolsetConfiguration;
    }

    private async Task<Config> LoadConfigurationAsync()
    {
        var settingsFileName = settingsFile.GetOrCreateFile();
        if (!fileSystem.FileExists(settingsFileName))
        {
            return new Config();
        }

        await using var stream = await fileSystem.OpenReadAsync(settingsFileName);
        var config = await JsonSerializer.DeserializeAsync<Config>(stream) ?? new Config();
        return config;
    }

    private Config LoadConfiguration()
    {
        var settingsFileName = settingsFile.GetOrCreateFile();
        if (!fileSystem.FileExists(settingsFileName))
        {
            return new Config();
        }

        using var stream = fileSystem.OpenRead(settingsFileName);
        var config = JsonSerializer.Deserialize<Config>(stream) ?? new Config();
        return config;
    }

    private class Config
    {
        public ToolsetConfiguration ToolsetConfiguration { get; set; } = new();
        public List<string> RecentFiles { get; init; } = [];
    }
}