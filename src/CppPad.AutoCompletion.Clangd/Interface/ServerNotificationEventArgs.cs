#region

using System.Text.Json;

#endregion

namespace CppPad.AutoCompletion.Clangd.Interface
{
    public class ServerNotificationEventArgs(JsonDocument jsonDoc) : EventArgs
    {
        public JsonDocument Message { get; } = jsonDoc;
    }
}