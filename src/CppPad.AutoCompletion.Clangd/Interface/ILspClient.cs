using System.Text.Json;

namespace CppPad.AutoCompletion.Clangd.Interface
{
    public interface ILspClient
    {
        Task InitializeAsync();

        int GetNextRequestId();

        Task SendMessageAsync(object message);

        Task<JsonDocument?> ReadResponseAsync(int expectedId);

        event EventHandler<ServerNotificationEventArgs>? OnServerNotification;
    }
}
