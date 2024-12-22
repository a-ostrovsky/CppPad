using System;
using System.Diagnostics;

namespace CppPad.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(OpenEditorsViewModel openEditorsViewModel, ToolbarViewModel toolbar)
    {
        OpenEditors = openEditorsViewModel;
        Toolbar = toolbar;
        Toolbar.CreateNewFileRequested += OnCreateNewFileRequested;
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(OpenEditorsViewModel.DesignInstance, ToolbarViewModel.DesignInstance);

    public ToolbarViewModel Toolbar { get; }

    public OpenEditorsViewModel OpenEditors { get; }

    private void OnCreateNewFileRequested(object? sender, EventArgs e)
    {
        var editor = OpenEditors.AddNewEditor();
        editor.CloseRequested += OnCloseRequested;
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