using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Tests.Fakes;

public class FakeDialogs : IDialogs
{
    private string? _fileName;

    private string? _inputBoxResponse;

    public Task NotifyErrorAsync(string message, Exception? exception)
    {
        return Task.CompletedTask;
    }

    public void NotifyError(string message, Exception? exception)
    {
    }

    public Task<string?> ShowFileOpenDialogAsync(string filter)
    {
        return Task.FromResult(_fileName);
    }

    public Task<string?> ShowFileSaveDialogAsync(string filter)
    {
        return Task.FromResult(_fileName);
    }

    public Task<string?> InputBoxAsync(string prompt, string title, string defaultResponse = "")
    {
        return Task.FromResult(_inputBoxResponse);
    }

    public Task ShowScriptSettingsDialogAsync(ScriptSettingsViewModel viewModel)
    {
        return Task.CompletedTask;
    }

    public void WillSelectFileWithName(string fileName)
    {
        _fileName = fileName;
    }

    public void WillReturnInputBoxResponse(string? response)
    {
        _inputBoxResponse = response;
    }
}