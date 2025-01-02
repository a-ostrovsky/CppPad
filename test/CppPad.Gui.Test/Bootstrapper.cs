using CppPad.Gui.ViewModels;
using CppPad.MockSystemAdapter;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.Tests;

public class Bootstrapper : IDisposable
{
    public Bootstrapper()
    {
        ScriptLoader = new ScriptLoader(new ScriptSerializer(), FileSystem);
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        MainWindowViewModel = new MainWindowViewModel(
            OpenEditorsViewModel,
            ToolbarViewModel,
            Dialogs);
    }

    public FakeDialogs Dialogs { get; } = new();

    public InMemoryFileSystem FileSystem { get; } = new();

    public FakeBuilder Builder { get; } = new();

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
            Builder,
            new SourceCodeViewModel());
    }

    public void Dispose()
    {
        MainWindowViewModel.Dispose();
        GC.SuppressFinalize(this);
    }
}