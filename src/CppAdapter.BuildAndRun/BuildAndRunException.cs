namespace CppAdapter.BuildAndRun;

public class BuildAndRunException : Exception
{
    public BuildAndRunException()
    {
    }

    public BuildAndRunException(string message) : base(message)
    {
    }

    public BuildAndRunException(string message, Exception inner) : base(message, inner)
    {
    }
}