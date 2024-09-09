#region

using CppPad.CompilerAdapter.Interface;
using CppPad.Gui.ViewModels;
using System.Diagnostics;

#endregion

namespace CppPad.Gui.Test.Helpers;

public class EditorHelper(ObjectTree objectTree)
{
    public const string SampleSourceCode = "int main() { return 42; }";

    public EditorViewModel CreateValidScript()
    {
        var mainWindow = objectTree.MainWindowViewModel;
        Debug.Assert(mainWindow.Editors.Count == 1);
        var editor = mainWindow.Editors[0];
        Assert.False(editor.IsModified);
        editor.SourceCode = SampleSourceCode;
        editor.Toolset =
            new ToolsetViewModel(
                new Toolset("TestType", CpuArchitecture.X64, "TestName", "TestExe"));
        return editor;
    }
}