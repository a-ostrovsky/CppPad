namespace CppPad.Configuration.Interface;

public interface IConfigurationStore
{
    const int MaxRecentFiles = 15;

    event EventHandler<EventArgs> LastOpenedFileNamesChanged;

    Task<ToolsetConfiguration> GetToolsetConfigurationAsync();

    ToolsetConfiguration GetToolsetConfiguration();

    Task SaveToolsetConfigurationAsync(
        ToolsetConfiguration toolsetConfiguration);

    Task<IReadOnlyList<string>> GetLastOpenedFileNamesAsync();

    Task SaveLastOpenedFileNameAsync(string fileName);
}

public class DummyConfigurationStore : IConfigurationStore
{
    public ToolsetConfiguration GetToolsetConfiguration()
    {
        return new ToolsetConfiguration();
    }

    public event EventHandler<EventArgs>? LastOpenedFileNamesChanged;

    public Task<ToolsetConfiguration> GetToolsetConfigurationAsync()
    {
        return Task.FromResult(new ToolsetConfiguration());
    }

    public Task SaveToolsetConfigurationAsync(
        ToolsetConfiguration toolsetConfiguration)
    {
        LastOpenedFileNamesChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetLastOpenedFileNamesAsync()
    {
        return Task.FromResult<IReadOnlyList<string>>(new List<string>());
    }

    public Task SaveLastOpenedFileNameAsync(string fileName)
    {
        return Task.CompletedTask;
    }
}
