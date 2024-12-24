namespace CppPad.Gui.Tests;

public class FakeDialogs : IDialogs
{
    private string? _fileName;

    public Task NotifyErrorAsync(string message, Exception exception)
    {
        return Task.CompletedTask;
    }

    public void NotifyError(string message, Exception exception)
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

    public static FakeDialogs Use()
    {
        var dialogs = new FakeDialogs();
        Dialogs.Instance = dialogs;
        return dialogs;
    }

    public void WillSelectFileWithName(string fileName)
    {
        _fileName = fileName;
    }
}