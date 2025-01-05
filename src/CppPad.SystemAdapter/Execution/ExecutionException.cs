namespace CppPad.SystemAdapter.Execution;

public class ExecutionException : Exception
{
    public ExecutionException() { }

    public ExecutionException(string message)
        : base(message) { }

    public ExecutionException(string message, Exception inner)
        : base(message, inner) { }
}
