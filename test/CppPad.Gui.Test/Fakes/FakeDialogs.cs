using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Tests.Fakes;

public class FakeDialogs : IDialogs
{
    private string? _fileName;

    private bool? _yesNoCancelResponse;

    private string? _inputBoxResponse;

    public bool YesNoCancelResponseShown { get; set; }

    public Task NotifyErrorAsync(string message, Exception? exception)
    {
        return Task.CompletedTask;
    }

    public void NotifyError(string message, Exception? exception) { }

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

    public Task<bool?> ShowYesNoCancelDialogAsync(string message, string title)
    {
        YesNoCancelResponseShown = true;
        return Task.FromResult(_yesNoCancelResponse);
    }

    public void WillReturnYesNoCancelResponse(bool? response)
    {
        _yesNoCancelResponse = response;
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
