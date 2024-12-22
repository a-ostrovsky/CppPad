using System;

namespace CppPad.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly OpenEditorsViewModel _openEditorsViewModel;
    private readonly ToolbarViewModel _toolbar;

    public MainWindowViewModel(OpenEditorsViewModel openEditorsViewModel, ToolbarViewModel toolbar)
    {
        _openEditorsViewModel = openEditorsViewModel;
        _toolbar = toolbar;
        _toolbar.CreateNewFileRequested += OnCreateNewFileRequested;
    }

    private void OnCreateNewFileRequested(object? sender, EventArgs e)
    {
        _openEditorsViewModel.AddNewEditor();
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(OpenEditorsViewModel.DesignInstance, ToolbarViewModel.DesignInstance);

    public ToolbarViewModel Toolbar => _toolbar;

    public OpenEditorsViewModel OpenEditors => _openEditorsViewModel;
}