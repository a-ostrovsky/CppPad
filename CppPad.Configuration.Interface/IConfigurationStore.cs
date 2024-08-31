namespace CppPad.Configuration.Interface;

public interface IConfigurationStore
{
    Task<ToolsetConfiguration> GetToolsetConfigurationAsync();

    ToolsetConfiguration GetToolsetConfiguration();

    Task SaveToolsetConfigurationAsync(
        ToolsetConfiguration toolsetConfiguration);
}

public class DummyConfigurationStore : IConfigurationStore
{
    public ToolsetConfiguration GetToolsetConfiguration()
    {
        return new ToolsetConfiguration();
    }

    public Task<ToolsetConfiguration> GetToolsetConfigurationAsync()
    {
        return Task.FromResult(new ToolsetConfiguration());
    }

    public Task SaveToolsetConfigurationAsync(
        ToolsetConfiguration toolsetConfiguration)
    {
        return Task.CompletedTask;
    }
}
