#region

using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.Benchmark.Interface;

public record InitSettings
{
    public bool ForceReinstall { get; set; } = false;
}

public interface IBenchmark
{
    Task InitAsync(IInitCallbacks callbacks, InitSettings settings,
        CancellationToken token = default);

    Script ToBenchmark(Script script);
}

public class DummyBenchmark : IBenchmark
{
    public Script ToBenchmark(Script script)
    {
        return script;
    }

    public Task InitAsync(IInitCallbacks callbacks, InitSettings initSettings,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}