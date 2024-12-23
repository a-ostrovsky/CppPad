#region

using Avalonia.Controls;
using Avalonia.Interactivity;

#endregion

namespace CppPad.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Dialogs.SetMainWindow(this);
    }
}