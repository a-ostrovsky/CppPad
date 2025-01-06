#region

using System.Text.Json;

#endregion

namespace CppPad.LspClient;

public class ServerNotificationEventArgs(JsonDocument jsonDoc) : EventArgs
{
    public JsonDocument Message { get; } = jsonDoc;
}
