#region

using System;
using System.Diagnostics;
using Avalonia.Controls;
using CppPad.Gui.ViewModels;

#endregion

namespace CppPad.Gui.Views;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        InitializeComponent();
        Init();
    }

    public EditorView(EditorViewModel viewModel)
    {
        InitializeComponent();
        Init();
        DataContext = viewModel;
    }

    private void Init()
    {
        DataContextChanged += EditorView_DataContextChanged;
    }

    private async void EditorView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        if (DataContext is not EditorViewModel vm)
        {
            throw new InvalidOperationException("DataContext is not EditorViewModel");
        }
        
        var sourceCodeEditor = this.FindControl<SourceCodeEditorView>("SourceCodeEditor");
        Debug.Assert(sourceCodeEditor != null);
        
        vm.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(vm.CurrentFileUri) && vm.CurrentFileUri != null)
            {
                _ = sourceCodeEditor.RenameFileAsync(vm.CurrentFileUri.LocalPath);
            }
        };
        
        await sourceCodeEditor.SetAutoCompletionProviderAsync(vm.AutoCompletionProvider);

        vm.GoToLineRequested += (_, args) => { SourceCodeEditor.ScrollToLine(args.Line); };
    }
}