#region

using CppPad.Gui.UnitTest.Mocks;
using CppPad.Gui.ViewModels;
using CppPad.MockFileSystem;

#endregion

namespace CppPad.Gui.UnitTest.Helpers;

public class ObjectTree
{
    public ObjectTree()
    {
        FileSystem = new InMemoryFileSystem();
        ErrorHandler = new ErrorHandlerMock();
        AutoCompletion = new AutoCompletionMock();
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
        ComponentInstallationViewModel =
            new ComponentInstallationViewModel(AutoCompletion, Benchmark, Router,
                InstallationProgressWindowViewModelFactory);
        DefinitionsWindowViewModelFactory =
            new DefinitionsWindowViewModelFactoryForTest(FileSystem);
        EditorViewModelFactory = new EditorViewModelFactoryForTest(
            DefinitionsWindowViewModelFactory, TemplatesViewModel, Router,
            Compiler, ScriptLoader, ConfigurationStore);
        MainWindowViewModel = new MainWindowViewModel(
            ComponentInstallationViewModel,
            TemplatesViewModel,
            EditorViewModelFactory,
            Router, ConfigurationStore);
    }

    public InMemoryFileSystem FileSystem { get; }

    public DefinitionsWindowViewModelFactoryForTest DefinitionsWindowViewModelFactory { get; }

    public AutoCompletionMock AutoCompletion { get; }

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

    public ComponentInstallationViewModel ComponentInstallationViewModel { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public InMemoryScriptStore ScriptLoader { get; }

    public CompilerMock Compiler { get; }

    public InMemoryConfigurationStore ConfigurationStore { get; }
}