#region

using Avalonia.Controls;
using CppPad.Gui.ViewModels;
using System;

#endregion

namespace CppPad.Gui.Views;

public partial class InstallationProgressWindow : Window
{
    private InstallationProgressWindowViewModel? _previousViewModel;

    public InstallationProgressWindow()
    {
        InitializeComponent();
    }

    public InstallationProgressWindow(InstallationProgressWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_previousViewModel != null)
        {
            _previousViewModel.OnFinished -= OnFinished;
        }

        if (DataContext is InstallationProgressWindowViewModel newViewModel)
        {
            newViewModel.OnFinished += OnFinished;
            _previousViewModel = newViewModel;
        }
    }

    private void OnFinished(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (_previousViewModel != null)
        {
            _previousViewModel.OnFinished -= OnFinished;
        }
    }
}