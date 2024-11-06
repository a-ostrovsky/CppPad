using System.Text.Json;

namespace CppPad.AutoCompletion.Clangd.Interface
{
    public class ServerNotificationEventArgs(JsonDocument jsonDoc) : EventArgs
    {
        public JsonDocument Message { get; } = jsonDoc;
    }
}
