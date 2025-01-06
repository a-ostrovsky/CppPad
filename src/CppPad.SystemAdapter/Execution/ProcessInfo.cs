namespace CppPad.SystemAdapter.Execution;

public class ProcessInfo(System.Diagnostics.Process process) : IProcessInfo
{
    public bool HasExited => process.HasExited;

    public object GetProcessData()
    {
        return process;
    }

    public StreamReader GetStandardOutput()
    {
        return process.StandardOutput;
    }

    public StreamReader GetStandardError()
    {
        return process.StandardError;
    }

    public StreamWriter GetStandardInput()
    {
        return process.StandardInput;
    }
}
