#region

using CppPad.Gui.UnitTest.Mocks;
using CppPad.Gui.ViewModels;

#endregion

namespace CppPad.Gui.UnitTest.Helpers;

public class ObjectTree
{
    public ObjectTree()
    {
        TemplateLoader = new InMemoryTemplateStore();
        Router = new RouterMock();
        Compiler = new CompilerMock();
        ScriptLoader = new InMemoryScriptStore();
        TemplatesViewModel = new TemplatesViewModel(TemplateLoader);
        ConfigurationStore = new InMemoryConfigurationStore();
        MainWindowViewModel = new MainWindowViewModel(TemplatesViewModel,
            new EditorViewModelFactoryForTest(TemplatesViewModel, Router, Compiler, ScriptLoader),
            Router, ConfigurationStore);
    }

    public ErrorHandlerMock ErrorHandler { get; } = new();

    public InMemoryTemplateStore TemplateLoader { get; }

    public RouterMock Router { get; }

    public TemplatesViewModel TemplatesViewModel { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public InMemoryScriptStore ScriptLoader { get; }

    public CompilerMock Compiler { get; }

    public InMemoryConfigurationStore ConfigurationStore { get; }
}