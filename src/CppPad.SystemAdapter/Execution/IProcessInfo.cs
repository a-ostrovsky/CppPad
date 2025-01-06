namespace CppPad.SystemAdapter.Execution;

public interface IProcessInfo
{
    bool HasExited { get; }

    object GetProcessData();

    StreamReader GetStandardOutput();

    StreamReader GetStandardError();

    StreamWriter GetStandardInput();
}