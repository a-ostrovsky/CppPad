using CppPad.BuildSystem;
using CppPad.Gui.ViewModels;

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
    public async Task BuildAndRunAsync_output_messages_are_displayed()
    {
        _bootstrapper.Builder.SetOutputMessage("Output_1");
        _bootstrapper.Builder.SetErrorMessage("Error_1");
        await _bootstrapper.OpenEditorsViewModel.CurrentEditor!.BuildAndRunAsync(BuildMode.Debug);
        Assert.Contains(
            "Output_1",
            _bootstrapper.OpenEditorsViewModel.CurrentEditor!.CompilerOutput.Output
        );
        Assert.Contains(
            "Error_1",
            _bootstrapper.OpenEditorsViewModel.CurrentEditor!.CompilerOutput.Output
        );
    }

    [Fact]
    public async Task BuildAndRunAsync_application_is_started_after_build()
    {
        await _bootstrapper.OpenEditorsViewModel.CurrentEditor!.BuildAndRunAsync(BuildMode.Debug);
        Assert.True(_bootstrapper.Runner.WasRunCalled);
    }

    [Fact]
    public async Task BuildAndRunAsync_output_tabs_are_switched_when_needed()
    {
        var currentEditor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        var selectedTabIndices = new List<int> { currentEditor.SelectedTabIndex };
        currentEditor.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(currentEditor.SelectedTabIndex))
            {
                selectedTabIndices.Add(currentEditor.SelectedTabIndex);
            }
        };
        await _bootstrapper.OpenEditorsViewModel.CurrentEditor!.BuildAndRunAsync(BuildMode.Debug);
        Assert.Equal(
            [
                EditorViewModel.TabIndices.CompilerOutput,
                EditorViewModel.TabIndices.ApplicationOutput,
            ],
            selectedTabIndices
        );
    }
}
