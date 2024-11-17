#region

using CppPad.AutoCompletion.Interface;

#endregion

namespace CppPad.AutoCompletion.Clangd.Interface;

public interface IResponseReceiver
{
    Task<ServerCapabilities> ReadCapabilitiesAsync(int requestId);

    Task<AutoCompletionItem[]> ReadCompletionsAsync(int requestId);

    Task<PositionInFile[]> ReadDefinitionsAsync(int requestId);

    event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}