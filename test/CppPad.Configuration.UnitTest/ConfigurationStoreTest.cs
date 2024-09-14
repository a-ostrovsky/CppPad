#region

using CppPad.Configuration.Interface;
using CppPad.Configuration.Json;
using CppPad.MockFileSystem;

#endregion

namespace CppPad.Configuration.UnitTest;

public class ConfigurationStoreTest
{
    private readonly InMemoryFileSystem _fileSystem = new();
    private readonly ConfigurationStore _configurationStore;

    public ConfigurationStoreTest()
    {
        var settingsFile = new SettingsFile(_fileSystem);
        _configurationStore = new ConfigurationStore(settingsFile, _fileSystem);
    }

    [Fact]
    public async Task GetToolsetConfigurationAsync_ShouldReturnSavedConfiguration()
    {
        var toolsetConfiguration = new ToolsetConfiguration
        {
            Toolsets = [
                new Toolset(Guid.Empty, "Type1", "X86", "Name1", "Path1")
            ],
            DefaultToolsetId = Guid.NewGuid()
        };
        await _configurationStore.SaveToolsetConfigurationAsync(toolsetConfiguration);

        var config = await _configurationStore.GetToolsetConfigurationAsync();

        Assert.NotNull(config);
        Assert.Single(config.Toolsets);
        Assert.Equal("Type1", config.Toolsets[0].Type);
        Assert.Equal("Name1", config.Toolsets[0].Name);
        Assert.Equal("Path1", config.Toolsets[0].ExecutablePath);
        Assert.Equal("X86", config.Toolsets[0].TargetArchitecture);
        Assert.Equal(toolsetConfiguration.DefaultToolsetId, config.DefaultToolsetId);
    }
}