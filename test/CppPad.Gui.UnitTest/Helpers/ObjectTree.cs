#region

using CppPad.Gui.UnitTest.Mocks;
using CppPad.Gui.ViewModels;

#endregion

namespace CppPad.Gui.UnitTest.Helpers;

public class ObjectTree
{
    public ObjectTree()
    {
        ErrorHandler = new ErrorHandlerMock();
        Benchmark = new BenchmarkMock();
        TemplateLoader = new InMemoryTemplateStore();
        Router = new RouterMock();
        Compiler = new CompilerMock();
        ScriptLoader = new InMemoryScriptStore();
        ToolsetDetector = new ToolsetDetectorMock();
        ConfigurationStore = new InMemoryConfigurationStore();
        ToolsetEditorWindowViewModel =
            new ToolsetEditorWindowViewModel(ToolsetDetector, ConfigurationStore);
        TemplatesViewModel = new TemplatesViewModel(TemplateLoader);
        InstallationProgressWindowViewModelFactory =
            new InstallationProgressWindowViewModelFactoryForTest();
        BenchmarkViewModel =
            new BenchmarkViewModel(Benchmark, Router, InstallationProgressWindowViewModelFactory);
        EditorViewModelFactory = new EditorViewModelFactoryForTest(TemplatesViewModel, Router,
            Compiler, ScriptLoader, ConfigurationStore);
        MainWindowViewModel = new MainWindowViewModel(
            BenchmarkViewModel,
            TemplatesViewModel,
            EditorViewModelFactory,
            Router, ConfigurationStore);
    }

    public BenchmarkMock Benchmark { get; }

    public ErrorHandlerMock ErrorHandler { get; }

    public InMemoryTemplateStore TemplateLoader { get; }

    public RouterMock Router { get; }

    public EditorViewModelFactoryForTest EditorViewModelFactory { get; }

    public InstallationProgressWindowViewModelFactoryForTest
        InstallationProgressWindowViewModelFactory
    { get; }

    public ToolsetDetectorMock ToolsetDetector { get; }

    public ToolsetEditorWindowViewModel ToolsetEditorWindowViewModel { get; }

    public TemplatesViewModel TemplatesViewModel { get; }

    public BenchmarkViewModel BenchmarkViewModel { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public InMemoryScriptStore ScriptLoader { get; }

    public CompilerMock Compiler { get; }

    public InMemoryConfigurationStore ConfigurationStore { get; }
}