using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CppPad.Gui.Input;
using CppPad.Gui.ViewModels;
using CppPad.Gui.Views;

namespace CppPad.Gui;

public class Dialogs : IDialogs
{
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

    /// <summary>
    ///     Displays an error notification in a simple dialog window.
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

    public void NotifyError(string message, Exception? exception)
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

    /// <summary>
    ///     Displays a simple input dialog that collects one string from the user.
    ///     Returns the user’s input, or null if the dialog is canceled.
    /// </summary>
    /// <param name="prompt">Label/prompt displayed above the input box.</param>
    /// <param name="title">Text displayed in the dialog’s title bar.</param>
    /// <param name="defaultResponse">Default text initially placed in the input box.</param>
    /// <returns>User’s input or null if canceled.</returns>
    public virtual async Task<string?> InputBoxAsync(string prompt, string title, string defaultResponse = "")
    {
        // Create the controls:
        var promptTextBlock = new TextBlock { Text = prompt };
        var textBox = new TextBox { Text = defaultResponse };

        var okButton = new Button
            { Content = "OK", MinWidth = 80, HorizontalContentAlignment = HorizontalAlignment.Center };
        var cancelButton = new Button
            { Content = "Cancel", MinWidth = 80, HorizontalContentAlignment = HorizontalAlignment.Center };

        // Create window:
        var inputWindow = new Window
        {
            Title = title,
            Width = 400,
            Height = 125,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        // Wire up commands to close the dialog with the appropriate return value:
        okButton.Command = new RelayCommand(_ =>
        {
            // Close passing back the input text.
            inputWindow.Close(textBox.Text);
        });
        cancelButton.Command = new RelayCommand(_ =>
        {
            // Close passing back null (canceled).
            inputWindow.Close(null);
        });

        // Layout with a StackPanel containing: Prompt, TextBox, and a button row:
        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new Thickness(10)
        };
        contentPanel.Children.Add(promptTextBlock);
        contentPanel.Children.Add(textBox);

        // Button row:
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        contentPanel.Children.Add(buttonPanel);

        // Set the window’s content:
        inputWindow.Content = contentPanel;

        inputWindow.KeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                // Pressing Enter => close with the current text
                case Key.Enter:
                    e.Handled = true;
                    inputWindow.Close(textBox.Text);
                    break;
                // Pressing Esc => close with null (canceled)
                case Key.Escape:
                    e.Handled = true;
                    inputWindow.Close(null);
                    break;
            }
        };

        inputWindow.Loaded += (_, _) =>
        {
            textBox.SelectAll();
            textBox.Focus();
        };

        // Show as a modal dialog returning a string (or null).
        return await inputWindow.ShowDialog<string?>(MainWindow);
    }

    public Task ShowScriptSettingsDialogAsync(ScriptSettingsViewModel viewModel)
    {
        var dialog = new ScriptSettingsWindow
        {
            DataContext = viewModel
        };
        return dialog.ShowDialog(MainWindow);
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