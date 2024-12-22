using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Bootstrapping;

public class GuiBootstrapper
{
    public GuiBootstrapper()
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