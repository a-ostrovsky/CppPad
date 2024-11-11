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

    public Task UpdateSettingsAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        return Task.FromResult(new ServerCapabilities());
    }

    public Task OpenFileAsync(ScriptDocument scriptDocument)
    {
        return Task.CompletedTask;
    }

    public Task CloseFileAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(ScriptDocument document, int line, int character)
    {
        return Task.FromResult(Array.Empty<AutoCompletionItem>());
    }

    public Task UpdateContentAsync(ScriptDocument document)
    {
        return Task.CompletedTask;
    }
}