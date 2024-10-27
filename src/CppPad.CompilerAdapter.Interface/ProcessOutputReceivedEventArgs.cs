namespace CppPad.CompilerAdapter.Interface;

public class ProcessOutputReceivedEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}