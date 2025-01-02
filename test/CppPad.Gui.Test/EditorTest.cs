namespace CppPad.Gui.Tests;

public class EditorTest : IDisposable
{
    private readonly Bootstrapper _bootstrapper = new();
    
    public void Dispose()
    {
        _bootstrapper.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task BuildAsync_output_messages_are_displayed()
    {
        _bootstrapper.Builder.SetOutputMessage("Output_1");
        _bootstrapper.Builder.SetErrorMessage("Error_1");
        await _bootstrapper.OpenEditorsViewModel.CurrentEditor!.BuildAndRunAsync();
        Assert.Contains("Output_1", _bootstrapper.OpenEditorsViewModel.CurrentEditor!.CompilerOutput.Output);
        Assert.Contains("Error_1", _bootstrapper.OpenEditorsViewModel.CurrentEditor!.CompilerOutput.Output);
    }
}