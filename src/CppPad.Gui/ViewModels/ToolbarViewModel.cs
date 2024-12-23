#region

using System;
using System.Windows.Input;

#endregion

namespace CppPad.Gui.ViewModels;

public class ToolbarViewModel : ViewModelBase
{
    public ToolbarViewModel()
    {
        CreateNewFileCommand = new RelayCommand(_ => CreateNewFileRequested?.Invoke(this, EventArgs.Empty));
        OpenFileCommand = new RelayCommand(_ => OpenFileRequested?.Invoke(this, EventArgs.Empty));
    }

    public ICommand OpenFileCommand { get; }

    public static ToolbarViewModel DesignInstance { get; } = new();

    public ICommand CreateNewFileCommand { get; }

    public event EventHandler? CreateNewFileRequested;

    public event EventHandler? OpenFileRequested;
}