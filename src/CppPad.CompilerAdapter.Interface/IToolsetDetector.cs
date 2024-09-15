namespace CppPad.CompilerAdapter.Interface;

public interface IToolsetDetector
{
    Task<IReadOnlyList<Toolset>> GetToolsetsAsync();
}

public class DummyToolsetDetector : IToolsetDetector
{
    public Task<IReadOnlyList<Toolset>> GetToolsetsAsync()
    {
        return Task.FromResult<IReadOnlyList<Toolset>>([
            new Toolset("Dummy", CpuArchitecture.X86, "Name", "ExePath")
        ]);
    }
}