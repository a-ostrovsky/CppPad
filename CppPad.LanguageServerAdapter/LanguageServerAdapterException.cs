namespace CppPad.LanguageServerAdapter;

public class LanguageServerAdapterException : Exception
{
    public LanguageServerAdapterException()
    {
    }

    public LanguageServerAdapterException(string message)
        : base(message)
    {
    }

    public LanguageServerAdapterException(string message, Exception inner)
        : base(message, inner)
    {
    }
}