#region

using System;
using System.Windows.Input;
using CppPad.Common;
using CppPad.Gui.Input;

#endregion

namespace CppPad.Gui.ViewModels;

public class ToolbarViewModel : ViewModelBase
{
    public ToolbarViewModel()
    {
        GoToLineCommand = new AsyncRelayCommand(_ => GoToLineRequested?.InvokeAsync(this, EventArgs.Empty));
        CreateNewFileCommand = new RelayCommand(_ => CreateNewFileRequested?.Invoke(this, EventArgs.Empty));
        OpenFileCommand = new AsyncRelayCommand(_ => OpenFileRequested?.InvokeAsync(this, EventArgs.Empty));
        SaveFileCommand = new AsyncRelayCommand(_ => SaveFileRequested?.InvokeAsync(this, EventArgs.Empty));
        SaveFileAsCommand = new AsyncRelayCommand(_ => SaveFileAsRequested?.InvokeAsync(this, EventArgs.Empty));
    }

    public IAsyncCommand GoToLineCommand { get; }

    public IAsyncCommand OpenFileCommand { get; }

    public IAsyncCommand SaveFileCommand { get; }

    public IAsyncCommand SaveFileAsCommand { get; }

    public static ToolbarViewModel DesignInstance { get; } = new();

    public ICommand CreateNewFileCommand { get; }

    public event AsyncEventHandler? GoToLineRequested;

    public event AsyncEventHandler? SaveFileRequested;

    public event AsyncEventHandler? SaveFileAsRequested;

    public event EventHandler? CreateNewFileRequested;

    public event AsyncEventHandler? OpenFileRequested;
}