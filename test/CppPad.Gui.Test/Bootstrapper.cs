using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Tests;

public class Bootstrapper
{
    public Bootstrapper()
    {
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        MainWindowViewModel = new MainWindowViewModel(OpenEditorsViewModel, ToolbarViewModel);
    }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private static EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(new SourceCodeViewModel());
    }
}