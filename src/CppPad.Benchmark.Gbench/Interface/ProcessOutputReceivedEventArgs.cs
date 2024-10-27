namespace CppPad.Benchmark.Gbench.Interface;

public class ProcessOutputReceivedEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}