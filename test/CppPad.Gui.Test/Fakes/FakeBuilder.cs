using CppAdapter.BuildAndRun;
using CppPad.BuildSystem;

namespace CppPad.Gui.Tests.Fakes;

public class FakeBuilder : IBuilder
{
    private string? _errorMessage;
    private string? _outputMessage;

    public Task<BuildSuccessResult> BuildAsync(
        BuildConfiguration buildConfiguration,
        CancellationToken token = default
    )
    {
        BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(BuildStatus.Building));

        if (_outputMessage != null)
        {
            buildConfiguration.ProgressReceived(
                this,
                new ProgressReceivedEventArgs(_outputMessage)
            );
        }

        if (_errorMessage != null)
        {
            buildConfiguration.ErrorReceived.Invoke(
                this,
                new ErrorReceivedEventArgs(_errorMessage)
            );
        }

        BuildStatusChanged?.Invoke(this, new BuildStatusChangedEventArgs(BuildStatus.Succeeded));

        return Task.FromResult(new BuildSuccessResult { CreatedFile = "C:\\x.exe" });
    }

    public event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;

    public void SetOutputMessage(string message)
    {
        _outputMessage = message;
    }

    public void SetErrorMessage(string message)
    {
        _errorMessage = message;
    }
}
