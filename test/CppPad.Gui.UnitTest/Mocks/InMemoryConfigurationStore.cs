#region

using CppPad.Configuration.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class InMemoryConfigurationStore : IConfigurationStore
{
    private readonly List<string> _recentFiles = [];
    private ToolsetConfiguration _toolsetConfiguration = new();

    public event EventHandler<EventArgs>? LastOpenedFileNamesChanged;

    public Task<ToolsetConfiguration> GetToolsetConfigurationAsync()
    {
        return Task.FromResult(_toolsetConfiguration);
    }

    public ToolsetConfiguration GetToolsetConfiguration()
    {
        return _toolsetConfiguration;
    }

    public Task SaveToolsetConfigurationAsync(ToolsetConfiguration toolsetConfiguration)
    {
        _toolsetConfiguration = toolsetConfiguration;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetLastOpenedFileNamesAsync()
    {
        return Task.FromResult<IReadOnlyList<string>>(_recentFiles);
    }

    public Task SaveLastOpenedFileNameAsync(string fileName)
    {
        _recentFiles.Remove(fileName);
        _recentFiles.Insert(0, fileName);
        if (_recentFiles.Count > IConfigurationStore.MaxRecentFiles)
        {
            _recentFiles.RemoveAt(IConfigurationStore.MaxRecentFiles);
        }

        LastOpenedFileNamesChanged?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }
}