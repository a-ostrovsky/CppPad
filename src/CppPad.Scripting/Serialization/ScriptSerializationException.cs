namespace CppPad.Scripting.Serialization;

public class ScriptSerializationException : Exception
{
    public ScriptSerializationException(string message)
        : base(message) { }

    public ScriptSerializationException(string message, Exception innerException)
        : base(message, innerException) { }
}
