namespace CppPad.BuildSystem.CMakeAdapter.Execution;

public class ProgressReceivedEventArgs(string data) : EventArgs
{
    public string Data { get; } = data;
}