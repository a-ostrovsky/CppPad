namespace CppPad.CompilerAdapter.Interface;
public interface IToolsetDetector
{
    Task<ICollection<Toolset>> GetToolsetsAsync();
}

public class DummyToolsetDetector : IToolsetDetector
{
    public Task<ICollection<Toolset>> GetToolsetsAsync()
    {
        return Task.FromResult<ICollection<Toolset>>([new("Dummy", "Name", "ExePath")]);
    }
}
