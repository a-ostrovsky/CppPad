using CppAdapter.BuildAndRun;
using CppPad.MockSystemAdapter;

namespace CppPad.BuildAndRun.Test;

public class RunnerTest
{
    [Fact]
    public async Task RunAsync_executes_process()
    {
        // Arrange
        var process = new FakeProcess();
        var runner = new Runner(process);
        var runConfiguration = new RunConfiguration
        {
            ExecutablePath = "test.exe",
            Arguments = ["arg1", "arg2"],
            OutputReceived = (_, e) =>
            {
                Assert.Equal("output", e.Data);
            },
            ErrorReceived = (_, e) =>
            {
                Assert.Equal("error", e.Data);
            },
        };

        // Act
        var runTask = runner.RunAsync(runConfiguration, CancellationToken.None);

        // Simulate process output and error
        process.RaiseOutputData("output");
        process.RaiseErrorData("error");

        await runTask;

        // Assert
        Assert.True(process.StartCalled);
        Assert.Contains(process.CapturedStartInfo, info => info.FileName == "test.exe");
        Assert.Contains(
            process.CapturedStartInfo,
            info => info.Arguments.SequenceEqual(["arg1", "arg2"])
        );
    }
}
