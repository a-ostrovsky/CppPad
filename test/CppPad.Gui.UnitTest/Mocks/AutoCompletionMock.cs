#region

using CppPad.AutoCompletion.Interface;
using CppPad.ScriptFile.Interface;

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

    public Task UpdateSettingsAsync(string filePath, Script script)
    {
        return Task.CompletedTask;
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

    public Task UpdateContentAsync(string filePath, string content)
    {
        return Task.CompletedTask;
    }
}