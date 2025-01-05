#region

using System;
using Avalonia.Controls;
using CppPad.Gui.ViewModels;
using CppPad.Logging;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.Gui.Views;

public partial class MainWindow : Window
{
    private readonly ILogger _logger = Log.CreateLogger<MainWindow>();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel mainWindowViewModel)
            {
                return;
            }

            if (mainWindowViewModel.OpenEditors.Editors.Count == 0)
            {
                return;
            }

            e.Cancel = true;
            await mainWindowViewModel.CloseAllEditorsAsync();

            if (mainWindowViewModel.OpenEditors.Editors.Count == 0)
            { // Close the window if all editors are closed
                Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to close all editors. Exception: {exception}", ex);
        }
    }
}
