namespace CppPad.CompilerAdapter.Interface;

public class ErrorReceivedEventArgs(string error) : EventArgs
{
    public string Error { get; } = error;
}
