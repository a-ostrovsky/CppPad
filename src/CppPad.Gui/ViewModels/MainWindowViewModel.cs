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
        CreateNewEditor();
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
                var editor = CreateNewEditor();
                await editor.OpenFileAsync(fileName);
            });
        }
        catch (Exception ex)
        {
            Dialogs.Instance.NotifyError("Failed to open file.", ex);
        }
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(OpenEditorsViewModel.DesignInstance, ToolbarViewModel.DesignInstance);

    public ToolbarViewModel Toolbar { get; }

    public OpenEditorsViewModel OpenEditors { get; }

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