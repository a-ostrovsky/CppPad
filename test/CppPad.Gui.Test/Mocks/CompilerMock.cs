#region

using CppPad.CompilerAdapter.Interface;

#endregion

namespace CppPad.Gui.Test.Mocks;

public class CompilerMock : ICompiler, IExecutable
{
    private Toolset? _lastToolset;
    private bool _runExecuted;
    private bool _shouldGenerateError;
    public string[] AdditionalPaths { get; private set; } = [];
    public event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    public Task<IExecutable> BuildAsync(Toolset toolset, BuildArgs args)
    {
        // Simulate build process
        _lastToolset = toolset;
        if (_shouldGenerateError)
        {
            CompilerMessageReceived?.Invoke(this,
                new CompilerMessageEventArgs(CompilerMessageType.Error, "Build failed"));
            return Task.FromException<IExecutable>(new CompilationFailedException());
        }

        CompilerMessageReceived?.Invoke(this,
            new CompilerMessageEventArgs(CompilerMessageType.Info, "Build started"));
        CompilerMessageReceived?.Invoke(this,
            new CompilerMessageEventArgs(CompilerMessageType.Info, "Build completed"));
        return Task.FromResult<IExecutable>(this);
    }

    public event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    public event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    public event EventHandler<ProcessExitedEventArgs>? ProcessExited;

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

    public void SetAdditionalEnvironmentPaths(IEnumerable<string> paths)
    {
        AdditionalPaths = paths.ToArray();
    }

    public void SetError()
    {
        _shouldGenerateError = true;
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