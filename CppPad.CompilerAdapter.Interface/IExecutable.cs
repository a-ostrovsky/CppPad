namespace CppPad.CompilerAdapter.Interface;

public interface IExecutable
{
    event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    Task RunAsync();
}

public class DummyExecutable : IExecutable
{
    public event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    public event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    public event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    public Task RunAsync()
    {
        return Task.CompletedTask;
    }
}
