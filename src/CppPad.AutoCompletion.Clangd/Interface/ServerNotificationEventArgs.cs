namespace CppPad.AutoCompletion.Clangd.Interface
{
    public class ServerNotificationEventArgs(string message) : EventArgs
    {
        public string Message { get; } = message;
    }
}
