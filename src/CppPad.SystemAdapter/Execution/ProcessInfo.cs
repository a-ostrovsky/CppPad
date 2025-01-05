namespace CppPad.SystemAdapter.Execution;

public class ProcessInfo(System.Diagnostics.Process process) : IProcessInfo
{
    public object GetProcessData()
    {
        return process;
    }
}
