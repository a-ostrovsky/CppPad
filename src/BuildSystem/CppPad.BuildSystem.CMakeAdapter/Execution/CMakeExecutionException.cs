namespace CppPad.BuildSystem.CMakeAdapter.Execution;

public class CMakeExecutionException : Exception
{
    public CMakeExecutionException()
    {
    }

    public CMakeExecutionException(string message) : base(message)
    {
    }

    public CMakeExecutionException(string message, Exception inner) : base(message, inner)
    {
    }
}