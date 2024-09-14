namespace CppPad.CompilerAdapter.Interface;

public interface IExecutable
{
    event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    Task RunAsync();

    void SetAdditionalEnvironmentPaths(IEnumerable<string> paths);
}

public class DummyExecutable : IExecutable
{
    public event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    public event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    public event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    public Task RunAsync()
    {
        OutputReceived?.Invoke(this, new OutputReceivedEventArgs(string.Empty));
        ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(string.Empty));
        ProcessExited?.Invoke(this, new ProcessExitedEventArgs(0));
        return Task.CompletedTask;
    }

    public void SetAdditionalEnvironmentPaths(IEnumerable<string> paths)
    {
    }
}
