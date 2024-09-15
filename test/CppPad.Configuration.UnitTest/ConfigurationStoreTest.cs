#region

using CppPad.Configuration.Interface;
using CppPad.Configuration.Json;
using CppPad.MockFileSystem;
using DeepEqual.Syntax;

#endregion

namespace CppPad.Configuration.UnitTest;

public class ConfigurationStoreTest
{
    private readonly ConfigurationStore _configurationStore;
    private readonly InMemoryFileSystem _fileSystem = new();

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
            Toolsets =
            [
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

        // ReSharper disable once MethodHasAsyncOverload
        _configurationStore.GetToolsetConfiguration().ShouldDeepEqual(config);
    }

    [Fact]
    public async Task SaveLastOpenedFileNameAsync_ShouldStoreRecentFiles()
    {
        // Arrange
        var fileNames = Enumerable
            .Range(1, 5 + IConfigurationStore.MaxRecentFiles)
            .Select(i => $"File{i}.txt").ToList();

        // Act
        foreach (var fileName in fileNames)
        {
            await _configurationStore.SaveLastOpenedFileNameAsync(fileName);
        }

        var recentFiles = await _configurationStore.GetLastOpenedFileNamesAsync();

        // Assert
        Assert.Equal(IConfigurationStore.MaxRecentFiles, recentFiles.Count);
        Assert.Equal(fileNames.Skip(5).Reverse(), recentFiles);
    }

    [Fact]
    public async Task SaveLastOpenedFileNameAsync_ShouldNotStoreSameFileMultipleTimes()
    {
        // Arrange
        const string fileName = "File1.txt";

        // Act
        await _configurationStore.SaveLastOpenedFileNameAsync(fileName);
        await _configurationStore.SaveLastOpenedFileNameAsync(fileName);
        await _configurationStore.SaveLastOpenedFileNameAsync(fileName);

        var recentFiles = await _configurationStore.GetLastOpenedFileNamesAsync();

        // Assert
        Assert.Single(recentFiles);
        Assert.Equal(fileName, recentFiles[0]);
    }
}