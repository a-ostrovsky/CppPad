#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CppPad.BuildSystem;
using CppPad.Common;
using CppPad.Gui.Input;

#endregion

namespace CppPad.Gui.ViewModels;

public class ToolbarViewModel : ViewModelBase
{
    private Configuration _selectedConfiguration;

    public ToolbarViewModel()
    {
        GoToLineCommand = new AsyncRelayCommand(_ => GoToLineRequested?.InvokeAsync(this, EventArgs.Empty));
        CreateNewFileCommand = new RelayCommand(_ => CreateNewFileRequested?.Invoke(this, EventArgs.Empty));
        OpenFileCommand = new AsyncRelayCommand(_ => OpenFileRequested?.InvokeAsync(this, EventArgs.Empty));
        SaveFileCommand = new AsyncRelayCommand(_ => SaveFileRequested?.InvokeAsync(this, EventArgs.Empty));
        SaveFileAsCommand = new AsyncRelayCommand(_ => SaveFileAsRequested?.InvokeAsync(this, EventArgs.Empty));
        BuildAndRunCommand = new AsyncRelayCommand(_ => BuildAndRunRequested?.InvokeAsync(this, EventArgs.Empty));
        CancelBuildAndRunCommand = new AsyncRelayCommand(_ => CancelBuildAndRunRequested?.InvokeAsync(this, EventArgs.Empty));
        OpenSettingsCommand = new AsyncRelayCommand(_ => OpenSettingsRequested?.InvokeAsync(this, EventArgs.Empty));
        SelectedConfiguration = Configurations[0];
    }

    public IAsyncCommand GoToLineCommand { get; }

    public IAsyncCommand OpenFileCommand { get; }

    public IAsyncCommand SaveFileCommand { get; }

    public IAsyncCommand SaveFileAsCommand { get; }

    public IAsyncCommand BuildAndRunCommand { get; }
    
    public IAsyncCommand CancelBuildAndRunCommand { get; }

    public static ToolbarViewModel DesignInstance { get; } = new();

    public ICommand CreateNewFileCommand { get; }

    public IAsyncCommand OpenSettingsCommand { get; }

    public event AsyncEventHandler? GoToLineRequested;

    public event AsyncEventHandler? SaveFileRequested;

    public event AsyncEventHandler? SaveFileAsRequested;

    public event AsyncEventHandler? BuildAndRunRequested;
    
    public event AsyncEventHandler? CancelBuildAndRunRequested;

    public event AsyncEventHandler? OpenSettingsRequested;

    public event EventHandler? CreateNewFileRequested;

    public event AsyncEventHandler? OpenFileRequested;
    
    public IReadOnlyList<Configuration> Configurations { get; } = Enum.GetValues<Configuration>();

    public Configuration SelectedConfiguration
    {
        get => _selectedConfiguration;
        set => SetProperty(ref _selectedConfiguration, value);
    }
}