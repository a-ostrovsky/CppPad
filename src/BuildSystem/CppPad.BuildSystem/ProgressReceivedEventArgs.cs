namespace CppPad.BuildSystem;

public class ProgressReceivedEventArgs(string data) : EventArgs
{
    public string Data { get; } = data;
}