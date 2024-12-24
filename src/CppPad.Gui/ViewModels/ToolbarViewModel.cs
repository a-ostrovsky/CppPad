#region

using System;
using System.Windows.Input;

#endregion

namespace CppPad.Gui.ViewModels;

public class ToolbarViewModel : ViewModelBase
{
    public ToolbarViewModel()
    {
        GoToLineCommand = new RelayCommand(_ => GoToLineRequested?.Invoke(this, EventArgs.Empty));
        CreateNewFileCommand = new RelayCommand(_ => CreateNewFileRequested?.Invoke(this, EventArgs.Empty));
        OpenFileCommand = new RelayCommand(_ => OpenFileRequested?.Invoke(this, EventArgs.Empty));
        SaveFileCommand = new RelayCommand(_ => SaveFileRequested?.Invoke(this, EventArgs.Empty));
        SaveFileAsCommand = new RelayCommand(_ => SaveFileAsRequested?.Invoke(this, EventArgs.Empty));
    }

    public ICommand GoToLineCommand { get; }

    public ICommand OpenFileCommand { get; }

    public ICommand SaveFileCommand { get; }

    public ICommand SaveFileAsCommand { get; }

    public static ToolbarViewModel DesignInstance { get; } = new();

    public ICommand CreateNewFileCommand { get; }

    public event EventHandler? GoToLineRequested;

    public event EventHandler? SaveFileRequested;

    public event EventHandler? SaveFileAsRequested;

    public event EventHandler? CreateNewFileRequested;

    public event EventHandler? OpenFileRequested;
}