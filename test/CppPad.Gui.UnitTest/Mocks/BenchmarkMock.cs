#region

using CppPad.Benchmark.Interface;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class BenchmarkMock : IBenchmark
{
    public Task InitializeAsync(IInitCallbacks callbacks, InitSettings settings,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Script ToBenchmark(Script script)
    {
        return script;
    }
}