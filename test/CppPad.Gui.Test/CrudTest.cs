﻿namespace CppPad.Gui.Tests;

public class CrudTest
{
    private readonly Bootstrapper _bootstrapper = new();

    private void CloseAllEditors()
    {
        _bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor?.CloseCommand.Execute(null);
    }

    [Fact]
    public void Create_new_file_upon_startup()
    {
        // Assert
        Assert.Single(_bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(_bootstrapper.OpenEditorsViewModel.Editors[0], _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }

    [Fact]
    public void CreateNewFile_creates_new_tab()
    {
        // Arrange & Act
        CloseAllEditors();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);

        // Assert
        Assert.Single(_bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(_bootstrapper.OpenEditorsViewModel.Editors[0], _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }


    [Fact]
    public void CloseEditor_closes_current_editor_and_selects_next()
    {
        // Arrange
        CloseAllEditors();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        var editor1 = _bootstrapper.OpenEditorsViewModel.Editors[0];
        var editor2 = _bootstrapper.OpenEditorsViewModel.Editors[1];
        _bootstrapper.OpenEditorsViewModel.CurrentEditor = editor1;

        // Act
        editor1.CloseCommand.Execute(null);

        // Assert
        Assert.DoesNotContain(editor1, _bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(editor2, _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }

    [Fact]
    public void CloseEditor_closes_current_editor_and_selects_previous()
    {
        // Arrange
        CloseAllEditors();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        var editor1 = _bootstrapper.OpenEditorsViewModel.Editors[0];
        var editor2 = _bootstrapper.OpenEditorsViewModel.Editors[1];
        _bootstrapper.OpenEditorsViewModel.CurrentEditor = editor1;

        // Act
        editor2.CloseCommand.Execute(null);

        // Assert
        Assert.DoesNotContain(editor2, _bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(editor1, _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }

    [Fact]
    public void CloseEditor_closes_last_editor()
    {
        // Arrange
        CloseAllEditors();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        var editor = _bootstrapper.OpenEditorsViewModel.Editors[0];

        // Act
        editor.CloseCommand.Execute(null);

        // Assert
        Assert.Empty(_bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Null(_bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }

    [Fact]
    public async Task Load_form_file_and_close_afterwards()
    {
        var scriptDocument = Fixture.CreateScriptDocument();
        await _bootstrapper.ScriptLoader.SaveAsync(scriptDocument, @"C:\s.cpppad");

        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\s.cpppad");
        await _bootstrapper.ToolbarViewModel.OpenFileCommand.ExecuteAsync(null);

        Assert.Equal(_bootstrapper.OpenEditorsViewModel.CurrentEditor, _bootstrapper.OpenEditorsViewModel.Editors[^1]);
        Assert.Equal(_bootstrapper.OpenEditorsViewModel.CurrentEditor?.SourceCode.Content,
            scriptDocument.Script.Content);
        Assert.Contains("s.cpppad", _bootstrapper.OpenEditorsViewModel.CurrentEditor?.Title);

        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor;
        _bootstrapper.OpenEditorsViewModel.CurrentEditor?.CloseCommand.Execute(null);
        Assert.DoesNotContain(editor, _bootstrapper.OpenEditorsViewModel.Editors);
    }

    [Fact]
    public async Task SaveFile_saves_file_correctly()
    {
        // Arrange
        var scriptDocument = Fixture.CreateScriptDocument();
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.ScriptDocument = scriptDocument;
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\s.cpppad");

        // Act
        await _bootstrapper.ToolbarViewModel.SaveFileAsCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_bootstrapper.FileSystem.FileExists(@"C:\s.cpppad"));
        Assert.Contains("s.cpppad", editor.Title);
    }

    [Fact]
    public async Task SaveFile_acts_as_save_as_when_save_as_is_called_first_time()
    {
        // Arrange & Act
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\s.cpppad");
        await _bootstrapper.ToolbarViewModel.SaveFileCommand.ExecuteAsync(null);

        // Assert
        Assert.True(_bootstrapper.FileSystem.FileExists(@"C:\s.cpppad"));
    }

    [Fact]
    public async Task SaveFile_saves_to_same_file_after_calling_SaveAs()
    {
        // Arrange
        var scriptDocument = Fixture.CreateScriptDocument();
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.ScriptDocument = scriptDocument;

        // Act
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\s.cpppad");
        await _bootstrapper.ToolbarViewModel.SaveFileAsCommand.ExecuteAsync(null);
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\x.cpppad");
        await _bootstrapper.ToolbarViewModel.SaveFileCommand.ExecuteAsync(null);

        // Assert
        // Must not save to the new file.
        Assert.False(_bootstrapper.FileSystem.FileExists(@"C:\x.cpppad"));
    }
}