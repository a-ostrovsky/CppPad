using CppPad.Gui.ErrorHandling;

namespace CppPad.Gui.Test.Mocks;

public class ErrorHandlerMock : IErrorHandler
{
    public Task DisplayErrorMessageAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    public async Task RunWithErrorHandlingAsync(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (Exception e)
        {
            Assert.Fail($"Exception: {e}");
        }
    }
}