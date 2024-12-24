using CppPad.Gui.ViewModels;
using CppPad.MockFileSystem;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.Tests;

public class Bootstrapper
{
    public Bootstrapper()
    {
        ScriptLoader = new ScriptLoader(new ScriptSerializer(), FileSystem);
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel();
        MainWindowViewModel = new MainWindowViewModel(OpenEditorsViewModel, ToolbarViewModel);
    }

    public InMemoryFileSystem FileSystem { get; } = new();

    public ScriptLoader ScriptLoader { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(ScriptLoader, new SourceCodeViewModel());
    }
}