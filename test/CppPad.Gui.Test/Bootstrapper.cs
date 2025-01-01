using CppPad.Gui.ViewModels;
using CppPad.MockSystemAdapter;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.Tests;

public class Bootstrapper
{
    public Bootstrapper()
    {
        ScriptLoader = new ScriptLoader(new ScriptSerializer(), FileSystem);
        OpenEditors = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        MainWindowViewModel = new MainWindowViewModel(OpenEditors, ToolbarViewModel, Dialogs);
    }

    public FakeDialogs Dialogs { get; } = new();

    public InMemoryFileSystem FileSystem { get; } = new();

    public FakeBuilder Builder { get; } = new();

    public ScriptLoader ScriptLoader { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditors { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(ScriptLoader, Builder, new SourceCodeViewModel());
    }
}