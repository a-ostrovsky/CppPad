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
        SaveCommand = new RelayCommand(_ => SaveRequested?.Invoke(this, EventArgs.Empty));
        SaveAsCommand = new RelayCommand(_ => SaveAsRequested?.Invoke(this, EventArgs.Empty));
    }

    public ICommand OpenFileCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand SaveAsCommand { get; }

    public static ToolbarViewModel DesignInstance { get; } = new();

    public ICommand CreateNewFileCommand { get; }

    public event EventHandler? SaveRequested;

    public event EventHandler? SaveAsRequested;

    public event EventHandler? CreateNewFileRequested;

    public event EventHandler? OpenFileRequested;
}