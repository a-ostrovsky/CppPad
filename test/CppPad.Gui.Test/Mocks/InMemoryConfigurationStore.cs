using CppPad.Configuration.Interface;

namespace CppPad.Gui.Test.Mocks;

public class InMemoryConfigurationStore : IConfigurationStore
{
    private ToolsetConfiguration _toolsetConfiguration = new();

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
}