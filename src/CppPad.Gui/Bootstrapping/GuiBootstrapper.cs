using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Bootstrapping;

public class GuiBootstrapper
{
    private readonly Bootstrapper _parent;

    public GuiBootstrapper(Bootstrapper parent)
    {
        _parent = parent;
        Dialogs = new Dialogs();
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        MainWindowViewModel = new MainWindowViewModel(OpenEditorsViewModel,
            ToolbarViewModel,
            Dialogs);
    }

    public ScriptSettingsViewModel ScriptSettingsViewModel { get; }

    public IDialogs Dialogs { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(
            ScriptSettingsViewModel,
            _parent.ScriptingBootstrapper.ScriptLoader,
            _parent.BuildAndRunBootstrapper.Builder,
            new SourceCodeViewModel());
    }
}