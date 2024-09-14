namespace CppPad.CompilerAdapter.Interface;

public class CompilationFailedException : Exception
{
    public CompilationFailedException() : base("Compilation failed.")
    {
    }

    public CompilationFailedException(string message) : base(message)
    {
    }

    public CompilationFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
