using CppPad.Common;
using CppPad.Gui.AutoCompletion;
using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Bootstrapping;

public class GuiBootstrapper
{
    private readonly Bootstrapper _parent;

    public GuiBootstrapper(Bootstrapper parent)
    {
        _parent = parent;
        ScriptLoaderViewModel = new ScriptLoaderViewModel(
            _parent.ScriptingBootstrapper.ScriptLoader,
            _parent.EventingBootstrapper.EventBus
        );
        AutoCompletionAdapter = new AutoCompletionAdapter(
            _parent.Dialogs,
            _parent.CodeAssistanceBootstrapper.CodeAssistant,
            new Timer());
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel(parent.ConfigurationBootstrapper.RecentFiles);
        MainWindowViewModel = new MainWindowViewModel(
            OpenEditorsViewModel,
            ToolbarViewModel,
            _parent.Dialogs
        );
    }

    public IAutoCompletionAdapter AutoCompletionAdapter { get; }

    public ScriptLoaderViewModel ScriptLoaderViewModel { get; }

    public ScriptSettingsViewModel ScriptSettingsViewModel { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(
            ScriptSettingsViewModel,
            ScriptLoaderViewModel,
            _parent.BuildAndRunBootstrapper.BuildAndRunFacade,
            AutoCompletionAdapter,
            _parent.EventingBootstrapper.EventBus
        );
    }
}
