using CppPad.AutoCompletion.Interface;

namespace CppPad.AutoCompletion.Clangd.Interface;

public interface IResponseReceiver
{
    Task<ServerCapabilities> ReadCapabilitiesAsync(int requestId);
    
    Task<AutoCompletionItem[]> ReadCompletionsAsync(int requestId);
    
    event EventHandler<DiagnosticsReceivedEventArgs>? OnDiagnosticsReceived;
}