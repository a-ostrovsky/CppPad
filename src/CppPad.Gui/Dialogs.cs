using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace CppPad.Gui;

public interface IDialogs
{
    Task NotifyErrorAsync(string message, Exception exception);
    void NotifyError(string message, Exception exception);
    Task<string?> ShowFileOpenDialogAsync(string filter);
    Task<string?> ShowFileSaveDialogAsync(string filter);
}

public class Dialogs : IDialogs
{
    private static IDialogs? _instance = new Dialogs();

    private static Window MainWindow
    {
        get
        {
            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.MainWindow;
            if (mainWindow == null)
            {
                throw new InvalidOperationException("Main window is not set.");
            }

            return mainWindow;
        }
    }

    public static IDialogs Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("Dialogs is not initialized. Call SetMainWindow first.");
            }

            return _instance;
        }
        set => _instance = value;
    }

    /// <summary>
    /// Displays an error notification in a simple dialog window.
    /// </summary>
    /// <param name="message">A user-friendly error message.</param>
    /// <param name="exception">The exception that caused the error (optional).</param>
    public virtual Task NotifyErrorAsync(string message, Exception? exception)
    {
        // Combine custom message + exception details (if available).
        var fullMessage = string.IsNullOrWhiteSpace(exception?.Message)
            ? message
            : $"{message}\n\nException: {exception.Message}";

        var textBox = new TextBox
        {
            Text = fullMessage,
            TextWrapping = TextWrapping.Wrap
        };
        var button = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        // Create a simple Window to show the error.
        var errorWindow = new Window
        {
            Title = "Error",
            Width = 640,
            Height = 480,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                },
                Margin = new Thickness(16),
                Children = { textBox, button }
            }
        };
        
        Grid.SetRow(textBox, 0);
        Grid.SetRow(button, 1);
        button.Command = new RelayCommand(_ => errorWindow.Close());

        // Show as a modal dialog.
        return errorWindow.ShowDialog(MainWindow);
    }
    
    public void NotifyError(string message, Exception exception)
    {
        Task.Run(() => NotifyErrorAsync(message, exception));
    }

    public virtual async Task<string?> ShowFileOpenDialogAsync(string filter)
    {
        var storageProvider = TopLevel.GetTopLevel(MainWindow)!.StorageProvider;
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = ParseFilter(filter)
        });
        return result.Count == 0 ? null : result[0].Path.AbsolutePath;
    }

    public async Task<string?> ShowFileSaveDialogAsync(string filter)
    {
        var storageProvider = TopLevel.GetTopLevel(MainWindow)!.StorageProvider;
        var result = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = ParseFilter(filter)
        });
        return result?.Path.AbsolutePath;
    }

    private static List<FilePickerFileType> ParseFilter(string filter)
    {
        return filter.Split('|')
            .Select((f, i) => new { Filter = f, Index = i })
            .GroupBy(f => f.Index / 2)
            .Select(g => new FilePickerFileType(g.First().Filter)
            {
                Patterns = g.Last().Filter.Split(';').Select(ext => ext).ToList()
            })
            .ToList();
    }
}