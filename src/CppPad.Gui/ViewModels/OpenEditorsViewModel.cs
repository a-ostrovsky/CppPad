#region

using System;
using System.Collections.ObjectModel;

#endregion

namespace CppPad.Gui.ViewModels;

public class OpenEditorsViewModel(Func<EditorViewModel> editorViewModelFactory) : ViewModelBase
{
    private EditorViewModel? _currentEditor;

    public static OpenEditorsViewModel DesignInstance { get; } = new(() => EditorViewModel.DesignInstance)
    {
        Editors = { EditorViewModel.DesignInstance },
        CurrentEditor = EditorViewModel.DesignInstance
    };

    public ObservableCollection<EditorViewModel> Editors { get; } = [];

    public EditorViewModel? CurrentEditor
    {
        get => _currentEditor;
        set => SetProperty(ref _currentEditor, value);
    }

    public void AddNewEditor()
    {
        var editor = editorViewModelFactory();
        Editors.Add(editor);
        CurrentEditor = editor;
    }
}