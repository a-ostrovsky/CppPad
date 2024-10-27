#region

using CppPad.Benchmark.Interface;

#endregion

namespace CppPad.Benchmark.Gbench.Interface;

public interface IBenchmarkBuilder
{
    public event EventHandler<ProcessOutputReceivedEventArgs>? ProcessOutputReceived;

    Task BuildAsync(IInitCallbacks callbacks, string cmakePath, string sourceDir, string buildDir,
        CancellationToken token = default);
}