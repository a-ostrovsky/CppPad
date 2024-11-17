#region

using System;
using Avalonia.Controls;
using CppPad.Gui.ErrorHandling;
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
        SourceCodeEditor.DataContext = viewModel;
    }

    private void Init()
    {
        DataContextChanged += EditorView_DataContextChanged;
    }

    private void EditorView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }

        if (DataContext is not EditorViewModel vm)
        {
            throw new InvalidOperationException("DataContext is not EditorViewModel");
        }

        // For whatever reason the binding does not always work. :(
        SourceCodeEditor.SetViewModel(vm);
        vm.GoToLineRequested += (_, args) =>
        {
            SourceCodeEditor.ScrollTo(args.Line, args.Character);
        };
        vm.GoToDefinitionsRequested += async (_, _) =>
        {
            await ErrorHandler.Instance.RunWithErrorHandlingAsync(() =>
                SourceCodeEditor.ShowDefinitionsAsync());
        };
    }
}