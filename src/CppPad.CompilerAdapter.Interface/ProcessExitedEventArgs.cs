namespace CppPad.CompilerAdapter.Interface;

public class ProcessExitedEventArgs(int exitCode) : EventArgs
{
    public int ExitCode { get; } = exitCode;
}