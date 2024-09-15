#region

using CppPad.CompilerAdapter.Interface;
using CppPad.Configuration.Interface;
using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;
using CppPad.ScriptFileLoader.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class EditorViewModelFactoryForTest(
    TemplatesViewModel templatesViewModel,
    IRouter router,
    ICompiler compiler,
    IScriptLoader scriptLoader,
    IConfigurationStore configurationStore) : IEditorViewModelFactory
{
    public EditorViewModel Create()
    {
        return new EditorViewModel(templatesViewModel, router, compiler, scriptLoader, configurationStore);
    }
}