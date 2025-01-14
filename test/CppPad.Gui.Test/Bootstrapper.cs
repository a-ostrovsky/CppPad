using CppAdapter.BuildAndRun;
using CppPad.Configuration;
using CppPad.Gui.AutoCompletion;
using CppPad.Gui.Eventing;
using CppPad.Gui.Observers;
using CppPad.Gui.Tests.Fakes;
using CppPad.Gui.ViewModels;
using CppPad.MockSystemAdapter;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.Tests;

public class Bootstrapper : IDisposable
{
    public Bootstrapper()
    {
        Runner = new FakeRunner();
        EventBus = new EventBus(Dialogs);
        RecentFiles = new RecentFiles(FileSystem);
        BuildAndRunFacade = new BuildAndRunFacade(Builder, Runner);
        ScriptSerializer = new ScriptSerializer();
        ScriptLoader = new ScriptLoader(ScriptSerializer, FileSystem);
        CodeAssistant = new FakeCodeAssistant();
        RecentFilesObserver = new RecentFilesObserver(RecentFiles, EventBus);
        CodeAssistanceObserver = new CodeAssistanceObserver(CodeAssistant, EventBus);
        ScriptLoaderViewModel = new ScriptLoaderViewModel(ScriptLoader, EventBus);
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel(RecentFiles);
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        MainWindowViewModel = new MainWindowViewModel(
            OpenEditorsViewModel,
            ToolbarViewModel,
            Dialogs
        );

        RecentFilesObserver.Start();
        CodeAssistanceObserver.Start();
    }

    public EventBus EventBus { get; }

    public RecentFilesObserver RecentFilesObserver { get; }

    public CodeAssistanceObserver CodeAssistanceObserver { get; }

    public ScriptSerializer ScriptSerializer { get; }

    public BuildAndRunFacade BuildAndRunFacade { get; }

    public FakeDialogs Dialogs { get; } = new();

    public InMemoryFileSystem FileSystem { get; } = new();

    public FakeCodeAssistant CodeAssistant { get; }

    public RecentFiles RecentFiles { get; }

    public FakeBuilder Builder { get; } = new();

    public FakeRunner Runner { get; }

    public ScriptLoader ScriptLoader { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public ScriptSettingsViewModel ScriptSettingsViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    public ScriptLoaderViewModel ScriptLoaderViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(
            ScriptSettingsViewModel,
            ScriptLoaderViewModel,
            BuildAndRunFacade,
            new DummyAutoCompletionAdapter(),
            EventBus
        );
    }

    public void Dispose()
    {
        MainWindowViewModel.Dispose();
        RecentFilesObserver.Dispose();
        GC.SuppressFinalize(this);
    }
}
