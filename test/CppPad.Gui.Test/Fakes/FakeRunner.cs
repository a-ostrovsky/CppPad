using CppAdapter.BuildAndRun;

namespace CppPad.Gui.Tests.Fakes;

public class FakeRunner : IRunner
{
    public Task RunAsync(RunConfiguration runConfiguration, CancellationToken cancellationToken)
    {
        WasRunCalled = true;
        return Task.CompletedTask;
    }
    
    public bool WasRunCalled { get; private set; }
}