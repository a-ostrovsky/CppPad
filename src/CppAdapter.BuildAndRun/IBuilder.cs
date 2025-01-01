using CppPad.BuildSystem;
using CppPad.Scripting;

namespace CppAdapter.BuildAndRun;

public interface IBuilder
{
    Task BuildAsync(BuildConfiguration buildConfiguration, CancellationToken token = default);
}

public class DummyBuilder : IBuilder
{
    public Task BuildAsync(BuildConfiguration buildConfiguration, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}