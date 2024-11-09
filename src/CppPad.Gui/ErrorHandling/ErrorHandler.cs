using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CppPad.Gui.ErrorHandling;

public interface IErrorHandler
{
    Task DisplayErrorMessageAsync(Exception ex);

    Task RunWithErrorHandlingAsync(Func<Task> task);
}

public class ErrorHandler : IErrorHandler
{
    public static IErrorHandler Instance { get; set; } = new ErrorHandler();

    public async Task DisplayErrorMessageAsync(Exception ex)
    {
        const string title = "Error occurred";
        var message = $"""
                       An error occurred: {ex.Message}.
                       ------------------------------
                       Details: {ex}
                       """;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok,
                Icon.Error);
            return box.ShowAsync();
        });
    }

    public async Task RunWithErrorHandlingAsync(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (Exception e)
        {
            await DisplayErrorMessageAsync(e);
        }
    }
}