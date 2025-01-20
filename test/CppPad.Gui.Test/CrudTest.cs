namespace CppPad.Gui.Tests;

public class CrudTest : IDisposable
{
    private readonly Bootstrapper _bootstrapper = new();

    public void Dispose()
    {
        _bootstrapper.Dispose();
        GC.SuppressFinalize(this);
    }

    private Task CloseAllEditorsAsync()
    {
        return _bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor?.CloseCommand.ExecuteAsync(
                null
            ) ?? Task.CompletedTask;
    }

    [Fact]
    public void Create_new_file_upon_startup()
    {
        // Assert
        Assert.Single(_bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(
            _bootstrapper.OpenEditorsViewModel.Editors[0],
            _bootstrapper.OpenEditorsViewModel.CurrentEditor
        );
    }

    [Fact]
    public async Task CreateNewFile_creates_new_tab()
    {
        // Arrange & Act
        await CloseAllEditorsAsync();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);

        // Assert
        Assert.Single(_bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(
            _bootstrapper.OpenEditorsViewModel.Editors[0],
            _bootstrapper.OpenEditorsViewModel.CurrentEditor
        );
    }

    [Fact]
    public async Task CloseEditor_closes_current_editor_and_selects_next()
    {
        // Arrange
        await CloseAllEditorsAsync();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        var editor1 = _bootstrapper.OpenEditorsViewModel.Editors[0];
        var editor2 = _bootstrapper.OpenEditorsViewModel.Editors[1];
        _bootstrapper.OpenEditorsViewModel.CurrentEditor = editor1;

        // Act
        await editor1.CloseCommand.ExecuteAsync(null);

        // Assert
        Assert.DoesNotContain(editor1, _bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(editor2, _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }

    [Fact]
    public async Task CloseEditor_closes_current_editor_and_selects_previous()
    {
        // Arrange
        await CloseAllEditorsAsync();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        var editor1 = _bootstrapper.OpenEditorsViewModel.Editors[0];
        var editor2 = _bootstrapper.OpenEditorsViewModel.Editors[1];
        _bootstrapper.OpenEditorsViewModel.CurrentEditor = editor1;

        // Act
        await editor2.CloseCommand.ExecuteAsync(null);

        // Assert
        Assert.DoesNotContain(editor2, _bootstrapper.OpenEditorsViewModel.Editors);
        Assert.Equal(editor1, _bootstrapper.OpenEditorsViewModel.CurrentEditor);
    }

    [Fact]
    public async Task CloseEditor_closes_last_editor()
    {
        // Arrange
        await CloseAllEditorsAsync();
        _bootstrapper.MainWindowViewModel.Toolbar.CreateNewFileCommand.Execute(null);
        var editor = _bootstrapper.OpenEditorsViewModel.Editors[0];

        // Act
        await editor.CloseCommand.ExecuteAsync(null);

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

        Assert.Equal(
            _bootstrapper.OpenEditorsViewModel.CurrentEditor,
            _bootstrapper.OpenEditorsViewModel.Editors[^1]
        );
        Assert.Equal(
            _bootstrapper.OpenEditorsViewModel.CurrentEditor?.SourceCode.ScriptDocument.Script.Content,
            scriptDocument.Script.Content
        );
        Assert.Contains("s.cpppad", _bootstrapper.OpenEditorsViewModel.CurrentEditor?.Title);

        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor;
        Assert.NotNull(editor);
        await _bootstrapper.OpenEditorsViewModel.CurrentEditor!.CloseCommand.ExecuteAsync(null);
        Assert.DoesNotContain(editor, _bootstrapper.OpenEditorsViewModel.Editors);
    }

    [Fact]
    public async Task SaveFile_saves_file_correctly()
    {
        // Arrange
        var scriptDocument = Fixture.CreateScriptDocument();
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.ResetDocument(scriptDocument);
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
        editor.SourceCode.ResetDocument(scriptDocument);

        // Act
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\s.cpppad");
        await _bootstrapper.ToolbarViewModel.SaveFileAsCommand.ExecuteAsync(null);
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\x.cpppad");
        await _bootstrapper.ToolbarViewModel.SaveFileCommand.ExecuteAsync(null);

        // Assert
        // Must not save to the new file.
        Assert.False(_bootstrapper.FileSystem.FileExists(@"C:\x.cpppad"));
    }

    [Fact]
    public async Task Save_and_load_recent_file()
    {
        // Arrange
        _bootstrapper.Dialogs.WillSelectFileWithName(@"C:\s.cpppad");

        // Act & Assert
        await _bootstrapper.ToolbarViewModel.SaveFileCommand.ExecuteAsync(null);
        _bootstrapper.EventBus.WaitForProcessing();
        Assert.Contains(@"C:\s.cpppad", _bootstrapper.ToolbarViewModel.RecentFiles);
        await _bootstrapper.ToolbarViewModel.OpenFileCommand.ExecuteAsync(@"C:\s.cpppad");
        Assert.Contains("s.cpppad", _bootstrapper.OpenEditorsViewModel.CurrentEditor?.Title);
    }

    [Fact]
    public void Is_modified_notification_is_displayed()
    {
        var currentEditor = _bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor!;
        Assert.False(currentEditor.IsModified);
        var newContent = currentEditor.SourceCode.ScriptDocument.Script.Content + "X";
        currentEditor.SourceCode.ResetContent(newContent);
        Assert.True(currentEditor.IsModified);
        Assert.Contains("*", currentEditor.Title);
    }

    [Fact]
    public async Task Asking_user_to_save()
    {
        // Arrange
        var currentEditor = _bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor!;
        var newContent = currentEditor.SourceCode.ScriptDocument.Script.Content + "X";
        currentEditor.SourceCode.ResetContent(newContent);
        _bootstrapper.Dialogs.WillReturnYesNoCancelResponse(true);
        _bootstrapper.Dialogs.WillSelectFileWithName("C:\\s.cpppad");

        // Act
        await CloseAllEditorsAsync();

        // Assert
        Assert.True(_bootstrapper.FileSystem.FileExists("C:\\s.cpppad"));
    }

    [Fact]
    public async Task Asking_can_abort_save_and_they_say_no()
    {
        // Arrange
        var currentEditor = _bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor!;
        var newContent = currentEditor.SourceCode.ScriptDocument.Script.Content + "X";
        currentEditor.SourceCode.ResetContent(newContent);
        _bootstrapper.Dialogs.WillReturnYesNoCancelResponse(false);
        _bootstrapper.Dialogs.WillSelectFileWithName("C:\\s.cpppad");

        // Act
        await CloseAllEditorsAsync();

        // Assert
        Assert.False(_bootstrapper.FileSystem.FileExists("C:\\s.cpppad"));
    }

    [Fact]
    public async Task Asking_user_to_save_but_user_can_abort_save()
    {
        // Arrange
        var currentEditor = _bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor!;
        var newContent = currentEditor.SourceCode.ScriptDocument.Script.Content + "X";
        currentEditor.SourceCode.ResetContent(newContent);
        _bootstrapper.Dialogs.WillReturnYesNoCancelResponse(null);

        // Act
        await CloseAllEditorsAsync();

        // Assert
        // should still not close
        Assert.NotNull(_bootstrapper.MainWindowViewModel.OpenEditors.CurrentEditor);
    }

    [Fact]
    public async Task Recent_file_is_deleted_from_list_if_it_does_not_exist()
    {
        // Arrange
        await _bootstrapper.RecentFiles.AddAsync("DOES_NOT_EXIST");
        var editorCountBeforeOpening = _bootstrapper.OpenEditorsViewModel.Editors.Count;

        // Act
        await _bootstrapper.ToolbarViewModel.OpenRecentFileCommand.ExecuteAsync("DOES_NOT_EXIST");
        _bootstrapper.EventBus.WaitForProcessing();

        // Assert
        // File does not exist, so it should be removed from the recent files.
        Assert.DoesNotContain(
            "DOES_NOT_EXIST",
            await _bootstrapper.RecentFiles.LoadRecentFilesAsync()
        );
        var editorCountAfterFailedOpening = _bootstrapper.OpenEditorsViewModel.Editors.Count;
        Assert.Equal(editorCountBeforeOpening, editorCountAfterFailedOpening);
    }
}
