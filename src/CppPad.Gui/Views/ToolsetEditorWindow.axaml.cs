#region

using Avalonia.Controls;
using CppPad.Gui.ViewModels;

#endregion

namespace CppPad.Gui.Views;

public partial class ToolsetEditorWindow : Window
{
    public ToolsetEditorWindow()
    {
        InitializeComponent();
    }

    public ToolsetEditorWindow(ToolsetEditorWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}