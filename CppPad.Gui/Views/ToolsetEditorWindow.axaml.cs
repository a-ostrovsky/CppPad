using Avalonia.Controls;

using CppPad.Gui.ViewModels;

namespace CppPad.Gui;

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
