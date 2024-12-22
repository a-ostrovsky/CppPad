namespace CppPad.Gui.Tests;

public class MainWindowViewModelTests
{
    private readonly Bootstrapper _bootstrapper = new();

    [Fact]
    public void CreateNewFile_creates_new_tab()
    {
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        Assert.Single(_bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(_bootstrapper.OpenEditorsViewModel.Editors[0], _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }
}