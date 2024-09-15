#region

using CppPad.CompilerAdapter.Interface;
using CppPad.Gui.UnitTest.Helpers;
using CppPad.ScriptFile.Interface;
using System.Reactive.Linq;

#endregion

namespace CppPad.Gui.UnitTest;

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
    public async Task Create_from_template()
    {
        var templateScript = new Script { Content = "void main(){}" };
        await ObjectTree.TemplateLoader.SaveAsync("template", templateScript);

        // Create new file from template
        var mainWindow = ObjectTree.MainWindowViewModel;
        await mainWindow.CreateNewFileFromTemplateCommand.Execute("template");

        Assert.Equal(2, mainWindow.Editors.Count);
        Assert.Equal("void main(){}", mainWindow.Editors[1].SourceCode);
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

    [Fact]
    public async Task Edit_Toolsets()
    {
        ObjectTree.ToolsetDetector.SetToolsets([
            new Toolset("Type", CpuArchitecture.X64, "Name1", "ExePath"),
            new Toolset("Type", CpuArchitecture.X64, "Name2", "ExePath")
        ]);

        // Auto detect toolsets
        await ObjectTree.ToolsetEditorWindowViewModel.AutodetectToolsetsCommand.Execute();
        var toolsets = ObjectTree.ToolsetEditorWindowViewModel.Toolsets;
        Assert.Equal(2, toolsets.Count);
        Assert.Equal("Type", toolsets[0].Type);
        Assert.Equal(CpuArchitecture.X64, toolsets[0].TargetArchitecture);
        Assert.Equal("Name1", toolsets[0].Name);
        Assert.Equal("ExePath", toolsets[0].ExecutablePath);

        // Set default toolset
        ObjectTree.ToolsetEditorWindowViewModel.SetDefaultToolsetCommand.Execute(toolsets[1]);
        Assert.False(toolsets[0].IsDefault);
        Assert.True(toolsets[1].IsDefault);
        ObjectTree.ToolsetEditorWindowViewModel.SetDefaultToolsetCommand.Execute(toolsets[0]);
        Assert.False(toolsets[1].IsDefault);
        Assert.True(toolsets[0].IsDefault);
    }
}