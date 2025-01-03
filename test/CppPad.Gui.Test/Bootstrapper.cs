using CppAdapter.BuildAndRun;
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
        BuildAndRunFacade = new BuildAndRunFacade(Builder, Runner);
        ScriptLoader = new ScriptLoader(new ScriptSerializer(), FileSystem);
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        MainWindowViewModel = new MainWindowViewModel(
            OpenEditorsViewModel,
            ToolbarViewModel,
            Dialogs);
    }

    public BuildAndRunFacade BuildAndRunFacade { get; }

    public FakeDialogs Dialogs { get; } = new();

    public InMemoryFileSystem FileSystem { get; } = new();

    public FakeBuilder Builder { get; } = new();
    
    public FakeRunner Runner { get; }

    public ScriptLoader ScriptLoader { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public ScriptSettingsViewModel ScriptSettingsViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(
            ScriptSettingsViewModel,
            ScriptLoader,
            BuildAndRunFacade,
            new SourceCodeViewModel());
    }

    public void Dispose()
    {
        MainWindowViewModel.Dispose();
        GC.SuppressFinalize(this);
    }
}