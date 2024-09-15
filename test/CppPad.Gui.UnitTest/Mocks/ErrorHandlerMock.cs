#region

using CppPad.Gui.ErrorHandling;

#endregion

namespace CppPad.Gui.Test.Mocks;

public class ErrorHandlerMock : IErrorHandler
{
    private bool _expectError;

    public Task DisplayErrorMessageAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    public async Task RunWithErrorHandlingAsync(Func<Task> task)
    {
        try
        {
            await task();
            Assert.False(_expectError);
        }
        catch (Exception e)
        {
            if (!_expectError)
            {
                Assert.Fail($"Exception: {e}");
            }
        }
    }

    public void ExpectError()
    {
        _expectError = true;
    }
}