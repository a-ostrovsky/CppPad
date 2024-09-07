using CppPad.CompilerAdapter.Interface;

namespace CppPad.Gui.Test.Mocks;

public class CompilerMock : ICompiler, IExecutable
{
    public event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;
    public event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    public event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    public event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    private Toolset? _lastToolset;
    private bool _runExecuted;

    public Task<IExecutable> BuildAsync(Toolset toolset, BuildArgs args)
    {
        // Simulate build process
        _lastToolset = toolset;
        CompilerMessageReceived?.Invoke(this, new CompilerMessageEventArgs(CompilerMessageType.Info, "Build started"));
        CompilerMessageReceived?.Invoke(this, new CompilerMessageEventArgs(CompilerMessageType.Info, "Build completed"));
        return Task.FromResult<IExecutable>(this);
    }

    public Task RunAsync()
    {
        // Simulate running the executable
        return Task.Run(() =>
        {
            _runExecuted = true;
            OutputReceived?.Invoke(this, new OutputReceivedEventArgs("Execution started"));
            // Simulate some output
            OutputReceived?.Invoke(this, new OutputReceivedEventArgs("Execution output"));
            // Simulate process exit
            ProcessExited?.Invoke(this, new ProcessExitedEventArgs(0));
        });
    }

    // Method to verify that build was performed with the given toolset
    public void VerifyBuild(Toolset expectedToolset)
    {
        Assert.NotNull(_lastToolset);
        Assert.Equal(expectedToolset.Type, _lastToolset.Type);
        Assert.Equal(expectedToolset.Name, _lastToolset.Name);
        Assert.Equal(expectedToolset.ExecutablePath, _lastToolset.ExecutablePath);
    }

    // Method to verify that run was executed
    public void VerifyBuildOutputRun()
    {
        Assert.True(_runExecuted, "Run was not executed.");
    }
}