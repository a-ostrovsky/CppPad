#region

using CppPad.CompilerAdapter.Interface;
using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;
using CppPad.ScriptFileLoader.Interface;

#endregion

namespace CppPad.Gui.Test.Mocks;

public class EditorViewModelFactoryForTest(
    TemplatesViewModel templatesViewModel,
    IRouter router,
    ICompiler compiler,
    IScriptLoader scriptLoader) : IEditorViewModelFactory
{
    public EditorViewModel Create()
    {
        return new EditorViewModel(templatesViewModel, router, compiler, scriptLoader);
    }
}