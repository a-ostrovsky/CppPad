#region

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CppPad.Gui.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.Routing;

public class Router(IServiceProvider serviceProvider) : IRouter
{
    private Window? _mainWindow;

    public Task ShowDialogAsync<T>() where T : ViewModelBase
    {
        var viewModelType = typeof(T);
        var windowTypeName = viewModelType.Name.Replace("ViewModel", string.Empty);

        var windowType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SingleOrDefault(t => t.Name == windowTypeName);

        if (windowType == null)
        {
            throw new ArgumentException(
                $"No such window: {windowTypeName}. Did you forget to create the window?");
        }

        var window = (Window)serviceProvider.GetRequiredService(windowType);
        if (_mainWindow == null)
        {
            throw new InvalidOperationException("Main window is not set.");
        }

        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        return window.ShowDialog(_mainWindow);
    }

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    public async Task<bool> AskUserAsync(string title, string text)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, text, ButtonEnum.YesNo);
        var result = await box.ShowAsync();
        return result == ButtonResult.Yes;
    }

    public async Task<Uri?> ShowSaveFileDialogAsync(string filter)
    {
        if (_mainWindow == null)
        {
            throw new InvalidOperationException(
                "Main window is not set. Call SetMainWindow first.");
        }

        var storage = TopLevel.GetTopLevel(_mainWindow)!.StorageProvider;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = ParseFilter(filter),
            ShowOverwritePrompt = true
        });
        return file?.Path;
    }

    public async Task<Uri?> ShowOpenFileDialogAsync(string filter)
    {
        if (_mainWindow == null)
        {
            throw new InvalidOperationException(
                "Main window is not set. Call SetMainWindow first.");
        }

        var storage = TopLevel.GetTopLevel(_mainWindow)!.StorageProvider;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = ParseFilter(filter),
            AllowMultiple = false
        });
        return files.Count == 0 ? null : files[0].Path;
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