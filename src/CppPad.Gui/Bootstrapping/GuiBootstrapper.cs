using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Bootstrapping;

public class GuiBootstrapper
{
    private readonly Bootstrapper _parent;

    public GuiBootstrapper(Bootstrapper parent)
    {
        _parent = parent;
        Dialogs = new Dialogs();
        ScriptLoaderViewModel = new ScriptLoaderViewModel(
            _parent.ScriptingBootstrapper.ScriptLoader,
            _parent.EventingBootstrapper.EventBus
        );
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel(parent.ConfigurationBootstrapper.RecentFiles);
        MainWindowViewModel = new MainWindowViewModel(
            OpenEditorsViewModel,
            ToolbarViewModel,
            Dialogs
        );
    }

    public ScriptLoaderViewModel ScriptLoaderViewModel { get; }

    public ScriptSettingsViewModel ScriptSettingsViewModel { get; }

    public IDialogs Dialogs { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(
            ScriptSettingsViewModel,
            ScriptLoaderViewModel,
            _parent.BuildAndRunBootstrapper.BuildAndRunFacade,
            new SourceCodeViewModel()
        );
    }
}
