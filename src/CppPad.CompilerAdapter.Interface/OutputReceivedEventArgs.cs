namespace CppPad.CompilerAdapter.Interface;

public class OutputReceivedEventArgs(string output) : EventArgs
{
    public string Output { get; } = output;
}