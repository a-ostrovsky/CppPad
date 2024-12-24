using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Bootstrapping;

public class GuiBootstrapper
{
    private readonly Bootstrapper _parent;

    public GuiBootstrapper(Bootstrapper parent)
    {
        _parent = parent;
        Dialogs = new Dialogs();
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        MainWindowViewModel = new MainWindowViewModel(OpenEditorsViewModel, ToolbarViewModel, Dialogs);
    }
    
    public IDialogs Dialogs { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(_parent.ScriptingBootstrapper.ScriptLoader, new SourceCodeViewModel());
    }
}