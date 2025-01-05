#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CppPad.BuildSystem;
using CppPad.Common;
using CppPad.Configuration;
using CppPad.Gui.Input;
using CppPad.SystemAdapter.IO;

#endregion

namespace CppPad.Gui.ViewModels;

public class ToolbarViewModel : ViewModelBase, IDisposable
{
    private readonly RecentFiles _recentFiles;
    private BuildMode _selectedMode;

    public ToolbarViewModel(RecentFiles recentFiles)
    {
        _recentFiles = recentFiles;
        _recentFiles.RecentFilesChanged += RecentFiles_RecentFilesChanged;
        GoToLineCommand = new AsyncRelayCommand(_ => GoToLineRequested?.InvokeAsync(this, EventArgs.Empty));
        CreateNewFileCommand = new RelayCommand(_ => CreateNewFileRequested?.Invoke(this, EventArgs.Empty));
        OpenFileCommand =
            new AsyncRelayCommand(_ => OpenFileRequested?.InvokeAsync(this, new OpenFileRequestedEventArgs()));
        OpenRecentFileCommand = new AsyncRelayCommand(fileName =>
            OpenFileRequested?.InvokeAsync(this, new OpenFileRequestedEventArgs(fileName?.ToString())));
        SaveFileCommand = new AsyncRelayCommand(_ => SaveFileRequested?.InvokeAsync(this, EventArgs.Empty));
        SaveFileAsCommand = new AsyncRelayCommand(_ => SaveFileAsRequested?.InvokeAsync(this, EventArgs.Empty));
        BuildAndRunCommand = new AsyncRelayCommand(_ => BuildAndRunRequested?.InvokeAsync(this, EventArgs.Empty));
        CancelBuildAndRunCommand =
            new AsyncRelayCommand(_ => CancelBuildAndRunRequested?.InvokeAsync(this, EventArgs.Empty));
        OpenSettingsCommand = new AsyncRelayCommand(_ => OpenSettingsRequested?.InvokeAsync(this, EventArgs.Empty));
        SelectedBuildMode = BuildModes[0];
        _recentFiles.LoadRecentFilesAsync().ContinueWith(t =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                RecentFiles.Clear();
                foreach (var file in t.Result)
                {
                    RecentFiles.Add(file);
                }
            });
        });
    }

    private void RecentFiles_RecentFilesChanged(object? sender, RecentFilesChangedEventArgs e)
    {
        RecentFiles.Clear();
        foreach (var file in e.RecentFiles)
        {
            RecentFiles.Add(file);
        }
    }

    public IAsyncCommand GoToLineCommand { get; }

    public IAsyncCommand OpenFileCommand { get; }

    public IAsyncCommand SaveFileCommand { get; }

    public IAsyncCommand SaveFileAsCommand { get; }

    public IAsyncCommand BuildAndRunCommand { get; }

    public IAsyncCommand CancelBuildAndRunCommand { get; }

    public static ToolbarViewModel DesignInstance { get; } = new(new RecentFiles(new DiskFileSystem()));

    public ICommand CreateNewFileCommand { get; }

    public IAsyncCommand OpenSettingsCommand { get; }

    public IReadOnlyList<BuildMode> BuildModes { get; } = Enum.GetValues<BuildMode>();

    public BuildMode SelectedBuildMode
    {
        get => _selectedMode;
        set => SetProperty(ref _selectedMode, value);
    }

    public IAsyncCommand OpenRecentFileCommand { get; }

    public ObservableCollection<string> RecentFiles { get; } = [];

    public event AsyncEventHandler? GoToLineRequested;

    public event AsyncEventHandler? SaveFileRequested;

    public event AsyncEventHandler? SaveFileAsRequested;

    public event AsyncEventHandler? BuildAndRunRequested;

    public event AsyncEventHandler? CancelBuildAndRunRequested;

    public event AsyncEventHandler? OpenSettingsRequested;

    public event EventHandler? CreateNewFileRequested;

    public event AsyncEventHandler<OpenFileRequestedEventArgs>? OpenFileRequested;

    public void Dispose()
    {
        _recentFiles.RecentFilesChanged -= RecentFiles_RecentFilesChanged;
        GC.SuppressFinalize(this);
    }
}