using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IDialogs _dialogs;

    public MainWindowViewModel(
        OpenEditorsViewModel openEditors,
        ToolbarViewModel toolbar,
        IDialogs dialogs
    )
    {
        _dialogs = dialogs;
        OpenEditors = openEditors;
        Toolbar = toolbar;
        Toolbar.CreateNewFileRequested += OnCreateNewFileRequested;
        Toolbar.OpenFileRequested += OnOpenFileRequestedAsync;
        Toolbar.SaveFileAsRequested += OnSaveFileAsRequestedAsync;
        Toolbar.SaveFileRequested += OnSaveFileRequestedAsync;
        Toolbar.GoToLineRequested += OnGoToLineRequestedAsync;
        Toolbar.BuildAndRunRequested += OnBuildAndRunRequestedAsync;
        Toolbar.CancelBuildAndRunRequested += OnCancelBuildAndRunRequestedAsync;
        Toolbar.OpenSettingsRequested += OnOpenSettingsRequestedAsync;
        CreateNewEditor();
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(OpenEditorsViewModel.DesignInstance, ToolbarViewModel.DesignInstance, new Dialogs());

    public ToolbarViewModel Toolbar { get; }

    public OpenEditorsViewModel OpenEditors { get; }

    private async Task OnOpenSettingsRequestedAsync(object sender, EventArgs e)
    {
        await ShowSettingsAsync();
    }

    private async Task ShowSettingsAsync()
    {
        var editor = OpenEditors.CurrentEditor;
        if (editor == null)
        {
            return;
        }

        using var editScope = editor.ScriptSettings.StartEdit();
        await _dialogs.ShowScriptSettingsDialogAsync(editor.ScriptSettings);
        if (!editor.ScriptSettings.ShouldApplySettings)
        {
            editScope.Rollback();
            return;
        }

        editor.ApplySettings(editScope.Commit());
    }

    private async Task OnGoToLineRequestedAsync(object? sender, EventArgs e)
    {
        try
        {
            var editor = OpenEditors.CurrentEditor;
            if (editor == null)
            {
                return;
            }

            var lineCount = editor.SourceCode.Content.Split('\n').Length;
            var currentLineAndColumn =
                $"{editor.SourceCode.CurrentLine}:{editor.SourceCode.CurrentColumn}";
            var result = await _dialogs.InputBoxAsync(
                $"Line[:Column]. Lines: 1 - {lineCount}",
                "Go to Line:Column",
                currentLineAndColumn
            );
            if (result == null)
            {
                return;
            }

            var parts = result.Split(':');
            if (
                parts.Length == 0
                || !int.TryParse(parts[0], out var line)
                || line < 0
                || line > lineCount
            )
            {
                return; // Invalid input
            }

            var column = 1;

            if (
                parts.Length > 1
                && int.TryParse(parts[1], out var parsedColumn)
                && parsedColumn > 0
            )
            {
                var lineContent = editor.SourceCode.Content.Split('\n')[line - 1];
                column = Math.Min(parsedColumn, lineContent.Length);
            }

            editor.SourceCode.CurrentColumn = 1; // Set to 1 first to avoid caret position issues. The line can be too short.
            editor.SourceCode.CurrentLine = line;
            editor.SourceCode.CurrentColumn = column;
        }
        catch (Exception ex)
        {
            await _dialogs.NotifyErrorAsync("Failed to go to line.", ex);
        }
    }

    private Task OnSaveFileRequestedAsync(object? sender, EventArgs e)
    {
        try
        {
            var editor = OpenEditors.CurrentEditor;
            if (editor == null)
            {
                return Task.CompletedTask;
            }

            if (editor.SourceCode.ScriptDocument.FileName == null)
            {
                return OnSaveFileAsRequestedAsync(sender, e);
            }

            return editor.SaveFileAsync();
        }
        catch (Exception ex)
        {
            return _dialogs.NotifyErrorAsync("Failed to save file.", ex);
        }
    }

    private async Task OnSaveFileAsRequestedAsync(object? sender, EventArgs e)
    {
        try
        {
            var editor = OpenEditors.CurrentEditor;
            if (editor == null)
            {
                return;
            }

            await Dispatcher.UIThread.Invoke(async () =>
            {
                try
                {
                    var fileName = await _dialogs.ShowFileSaveDialogAsync(
                        Extensions.CppPadFileFilter
                    );
                    if (fileName == null)
                    {
                        return;
                    }

                    await editor.SaveFileAsAsync(fileName);
                }
                catch (Exception ex)
                {
                    await _dialogs.NotifyErrorAsync("Failed to save file.", ex);
                }
            });
        }
        catch (Exception ex)
        {
            await _dialogs.NotifyErrorAsync("Failed to open file.", ex);
        }
    }

    private async Task OnOpenFileRequestedAsync(object? sender, EventArgs e)
    {
        try
        {
            var fileName = await _dialogs.ShowFileOpenDialogAsync(Extensions.CppPadFileFilter);
            if (fileName == null)
            {
                return;
            }

            await Dispatcher.UIThread.Invoke(async () =>
            {
                try
                {
                    var editor = CreateNewEditor();
                    await editor.OpenFileAsync(fileName);
                }
                catch (Exception ex)
                {
                    await _dialogs.NotifyErrorAsync("Failed to open file.", ex);
                }
            });
        }
        catch (Exception ex)
        {
            await _dialogs.NotifyErrorAsync("Failed to open file.", ex);
        }
    }

    private void OnCreateNewFileRequested(object? sender, EventArgs e)
    {
        CreateNewEditor();
    }

    private EditorViewModel CreateNewEditor()
    {
        var editor = OpenEditors.AddNewEditor();
        editor.CloseRequested += OnCloseRequested;
        return editor;
    }

    private async Task OnCancelBuildAndRunRequestedAsync(object sender, EventArgs e)
    {
        var editor = OpenEditors.CurrentEditor;
        if (editor == null)
        {
            return;
        }

        await editor.CancelBuildAndRunAsync();
    }

    private async Task OnBuildAndRunRequestedAsync(object sender, EventArgs e)
    {
        var editor = OpenEditors.CurrentEditor;
        if (editor == null)
        {
            return;
        }

        try
        {
            await editor.BuildAndRunAsync(Toolbar.SelectedConfiguration);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (CMakeExecutionException)
        {
            // Ignore because the information is already displayed in the compiler output.
        }
        catch (Exception ex)
        {
            await _dialogs.NotifyErrorAsync("Failed to build and run.", ex);
        }
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        var editor = (EditorViewModel?)sender;
        Debug.Assert(editor != null);
        try
        {
            var index = OpenEditors.Editors.IndexOf(editor);
            OpenEditors.Editors.Remove(editor);

            if (OpenEditors.CurrentEditor != editor)
            {
                return;
            }

            if (OpenEditors.Editors.Count > 0)
            {
                // Select the next editor if available, otherwise select the previous one
                OpenEditors.CurrentEditor =
                    index < OpenEditors.Editors.Count
                        ? OpenEditors.Editors[index]
                        : OpenEditors.Editors[index - 1];
            }
            else
            {
                OpenEditors.CurrentEditor = null;
            }
        }
        finally
        {
            editor.Dispose();
        }
    }

    public void Dispose()
    {
        Toolbar.CreateNewFileRequested -= OnCreateNewFileRequested;
        Toolbar.OpenFileRequested -= OnOpenFileRequestedAsync;
        Toolbar.SaveFileAsRequested -= OnSaveFileAsRequestedAsync;
        Toolbar.SaveFileRequested -= OnSaveFileRequestedAsync;
        Toolbar.GoToLineRequested -= OnGoToLineRequestedAsync;
        Toolbar.BuildAndRunRequested -= OnBuildAndRunRequestedAsync;
        Toolbar.CancelBuildAndRunRequested -= OnCancelBuildAndRunRequestedAsync;
        Toolbar.OpenSettingsRequested -= OnOpenSettingsRequestedAsync;
        GC.SuppressFinalize(this);
    }
}
