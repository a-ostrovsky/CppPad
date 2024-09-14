#region

using CppPad.Gui.Test.Helpers;
using System.Reactive.Linq;

#endregion

namespace CppPad.Gui.Test;

public class WorkflowsTest : TestBase
{
    [Fact]
    public async Task Create_save_and_load_file()
    {
        // Create new file
        var mainWindow = ObjectTree.MainWindowViewModel;
        Assert.Single(mainWindow.Editors); // Expect already one file to be there
        await mainWindow.CreateNewFileCommand.Execute();
        Assert.Equal(2, mainWindow.Editors.Count);
        Assert.False(mainWindow.Editors[1].IsModified);

        // Close file
        await mainWindow.CloseEditorCommand.Execute(mainWindow.Editors[1]);
        Assert.Single(mainWindow.Editors);

        // Edit file
        var editor = mainWindow.Editors[0];
        editor.SourceCode = EditorHelper.SampleSourceCode;
        Assert.True(editor.IsModified);

        // Save file
        ObjectTree.Router.SetSelectedFile(new Uri("file:///C:/name%20with%20space.cpp"));
        await editor.SaveCommand.Execute();
        Assert.False(editor.IsModified);

        // Assert that file exists
        const string expectedFileNameWithoutPath = "name with space.cpp";
        var existingFileNames = ObjectTree.ScriptLoader.GetFileNames()
            .Select(Path.GetFileName)
            .ToArray();
        Assert.Contains(expectedFileNameWithoutPath, existingFileNames);

        // Load file
        await mainWindow.OpenFileCommand.Execute();
        Assert.Equal(EditorHelper.SampleSourceCode, editor.SourceCode);
        Assert.False(editor.IsModified);
    }

    [Fact]
    public async Task RunScript()
    {
        var editor = new EditorHelper(ObjectTree).CreateValidScript();
        await editor.RunCommand.Execute();
        var compiler = ObjectTree.Compiler;
        compiler.VerifyBuild(editor.Toolset!.ToCompilerToolset());
        compiler.VerifyBuildOutputRun();
    }

    [Fact]
    public async Task RunScript_WithCompileError()
    {
        ObjectTree.ErrorHandler.ExpectError();
        ObjectTree.Compiler.SetCompilerError();

        // Compile once
        var editor = new EditorHelper(ObjectTree).CreateValidScript();
        await editor.RunCommand.Execute();
        Assert.NotNull(editor.CompilerOutput);
        Assert.NotEmpty(editor.CompilerOutput);

        // Compile again, same output expected
        var previousOutput = editor.CompilerOutput;
        await editor.RunCommand.Execute();
        Assert.Equal(previousOutput, editor.CompilerOutput);
    }
}