using Avalonia.Controls;

using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;

namespace CppPad.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel, IRouter router)
    {
        router.SetMainWindow(this);
        DataContext = viewModel;
        InitializeComponent();
    }
}
