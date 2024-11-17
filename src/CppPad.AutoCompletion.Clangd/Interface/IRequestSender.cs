#region

using CppPad.AutoCompletion.Interface;

#endregion

namespace CppPad.AutoCompletion.Clangd.Interface;

public interface IRequestSender
{
    Task InitializeClientAsync();
    Task<int> SendInitializeRequestAsync(int processId, string rootUri);
    Task SendInitializedNotificationAsync();
    Task SendDidOpenAsync(string fileName, string text);
    Task SendDidChangeConfigurationAsync(IDictionary<string, object> settings);
    Task SendDidChangeAsync(string fileName, int version, string text);
    Task SendDidCloseAsync(string fileName);
    Task<int> SendCompletionRequestAsync(PositionInFile positionInFile);
    Task<int> SendFindDefinitionAsync(PositionInFile positionInFile);
}