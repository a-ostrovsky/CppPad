namespace CppPad.CompilerAdapter.Interface;

public class CompilerMessageEventArgs(
    CompilerMessageType messageType,
    string message) : EventArgs
{
    public CompilerMessageType MessageType { get; } = messageType;
    public string Message { get; } = message;
}