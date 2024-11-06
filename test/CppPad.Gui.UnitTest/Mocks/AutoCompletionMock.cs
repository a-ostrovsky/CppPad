#region

using CppPad.AutoCompletion.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class AutoCompletionMock : IAutoCompletionService, IAutoCompletionInstaller
{
    public Task InstallAsync(IInitCallbacks initCallbacks, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public bool IsClangdInstalled()
    {
        return true;
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return Task.FromResult(new ServerCapabilities());
    }

    public Task OpenFileAsync(string filePath, string fileContent)
    {
        return Task.CompletedTask;
    }

    public Task CloseFileAsync(string filePath)
    {
        return Task.CompletedTask;
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line, int character)
    {
        return Task.FromResult(Array.Empty<AutoCompletionItem>());
    }

    public Task DidChangeAsync(string filePath, string newText)
    {
        return Task.CompletedTask;
    }

    public Task RenameFileAsync(string oldFilePath, string newFilePath)
    {
        return Task.CompletedTask;
    }
}