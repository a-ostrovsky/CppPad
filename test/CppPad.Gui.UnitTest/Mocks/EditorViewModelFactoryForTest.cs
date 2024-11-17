#region

using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.Configuration.Interface;
using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class EditorViewModelFactoryForTest(
    IDefinitionsWindowViewModelFactory definitionsWindowViewModelFactory,
    TemplatesViewModel templatesViewModel,
    IRouter router,
    ICompiler compiler,
    IScriptLoader scriptLoader,
    IConfigurationStore configurationStore) : IEditorViewModelFactory
{
    public EditorViewModel Create()
    {
        return new EditorViewModel(
            definitionsWindowViewModelFactory,
            templatesViewModel,
            router,
            compiler,
            scriptLoader,
            configurationStore,
            new DummyAutoCompletionService(),
            new DummyTimer());
    }
}