using CppAdapter.BuildAndRun;
using CppPad.BuildSystem;

namespace CppPad.Gui.Tests;

public class FakeBuilder : IBuilder
{
    private string? _errorMessage;
    private string? _outputMessage;

    public Task BuildAsync(BuildConfiguration buildConfiguration, CancellationToken token = default)
    {
        if (_outputMessage != null)
        {
            buildConfiguration.ProgressReceived?.Invoke(this, new ProgressReceivedEventArgs(_outputMessage));
        }

        if (_errorMessage != null)
        {
            buildConfiguration.ErrorReceived?.Invoke(this, new ErrorReceivedEventArgs(_errorMessage));
        }

        return Task.CompletedTask;
    }

    public void SetOutputMessage(string message)
    {
        _outputMessage = message;
    }

    public void SetErrorMessage(string message)
    {
        _errorMessage = message;
    }
}