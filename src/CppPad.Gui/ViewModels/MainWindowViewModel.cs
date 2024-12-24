using System;
using System.Diagnostics;
using Avalonia.Threading;
using CppPad.FileSystem;

namespace CppPad.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(OpenEditorsViewModel openEditorsViewModel, ToolbarViewModel toolbar)
    {
        OpenEditors = openEditorsViewModel;
        Toolbar = toolbar;
        Toolbar.CreateNewFileRequested += OnCreateNewFileRequested;
        Toolbar.OpenFileRequested += OnOpenFileRequested;
        Toolbar.SaveFileAsRequested += OnSaveFileAsRequested;
        Toolbar.SaveFileRequested += OnSaveFileRequested;
        Toolbar.GoToLineRequested += OnGoToLineRequested;
        CreateNewEditor();
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(OpenEditorsViewModel.DesignInstance, ToolbarViewModel.DesignInstance);

    public ToolbarViewModel Toolbar { get; }

    public OpenEditorsViewModel OpenEditors { get; }

    private async void OnGoToLineRequested(object? sender, EventArgs e)
    {
        try
        {
            var editor = OpenEditors.CurrentEditor;
            if (editor == null)
            {
                return;
            }

            var lineCount = editor.SourceCode.Content.Split('\n').Length;
            var currentLineAndColumn = $"{editor.SourceCode.CurrentLine}:{editor.SourceCode.CurrentColumn}";
            var result = await Dialogs.Instance.InputBoxAsync(
                $"Line[:Column]. Lines: 1 - {lineCount}",
                "Go to Line:Column",
                currentLineAndColumn);
            if (result == null)
            {
                return;
            }

            var parts = result.Split(':');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var line) || line < 0 || line > lineCount)
            {
                return; // Invalid input
            }

            var column = 1;

            if (parts.Length > 1 && int.TryParse(parts[1], out var parsedColumn) && parsedColumn > 0)
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
            await Dialogs.Instance.NotifyErrorAsync("Failed to go to line.", ex);
        }
    }

    private void OnSaveFileRequested(object? sender, EventArgs e)
    {
        try
        {
            var editor = OpenEditors.CurrentEditor;
            if (editor == null)
            {
                return;
            }

            if (editor.SourceCode.ScriptDocument.FileName == null)
            {
                OnSaveFileAsRequested(sender, e);
                return;
            }

            editor.SaveFileAsync();
        }
        catch (Exception ex)
        {
            Dialogs.Instance.NotifyErrorAsync("Failed to save file.", ex);
        }
    }

    private async void OnSaveFileAsRequested(object? sender, EventArgs e)
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
                    var fileName = await Dialogs.Instance.ShowFileSaveDialogAsync(Extensions.CppPadFileFilter);
                    if (fileName == null)
                    {
                        return;
                    }

                    await editor.SaveFileAsAsync(fileName);
                }
                catch (Exception ex)
                {
                    await Dialogs.Instance.NotifyErrorAsync("Failed to save file.", ex);
                }
            });
        }
        catch (Exception ex)
        {
            await Dialogs.Instance.NotifyErrorAsync("Failed to open file.", ex);
        }
    }

    private async void OnOpenFileRequested(object? sender, EventArgs e)
    {
        try
        {
            var fileName = await Dialogs.Instance.ShowFileOpenDialogAsync(Extensions.CppPadFileFilter);
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
                    await Dialogs.Instance.NotifyErrorAsync("Failed to open file.", ex);
                }
            });
        }
        catch (Exception ex)
        {
            await Dialogs.Instance.NotifyErrorAsync("Failed to open file.", ex);
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

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        var editor = (EditorViewModel?)sender;
        Debug.Assert(editor != null);
        var index = OpenEditors.Editors.IndexOf(editor);
        OpenEditors.Editors.Remove(editor);

        if (OpenEditors.CurrentEditor != editor)
        {
            return;
        }

        if (OpenEditors.Editors.Count > 0)
        {
            // Select the next editor if available, otherwise select the previous one
            OpenEditors.CurrentEditor = index < OpenEditors.Editors.Count
                ? OpenEditors.Editors[index]
                : OpenEditors.Editors[index - 1];
        }
        else
        {
            OpenEditors.CurrentEditor = null;
        }
    }
}