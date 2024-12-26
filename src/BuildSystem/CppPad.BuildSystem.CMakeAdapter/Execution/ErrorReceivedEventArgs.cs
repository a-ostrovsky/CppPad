namespace CppPad.BuildSystem.CMakeAdapter.Execution;

public class ErrorReceivedEventArgs(string data) : EventArgs
{
    public string Data { get; } = data;
}