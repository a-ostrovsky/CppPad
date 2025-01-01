namespace CppPad.BuildSystem;

public class ErrorReceivedEventArgs(string data) : EventArgs
{
    public string Data { get; } = data;
}