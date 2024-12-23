using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CppPad.Gui.Views;

namespace CppPad.Gui;

public interface IDialogs
{
    void NotifyError(string message, Exception exception);
    Task<string?> ShowFileOpenDialogAsync(string filter);
}

public class Dialogs(MainWindow mainWindow) : IDialogs
{
    private static IDialogs? _instance;

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

    public virtual void NotifyError(string message, Exception exception)
    {
    }

    public virtual async Task<string?> ShowFileOpenDialogAsync(string filter)
    {
        var storageProvider = TopLevel.GetTopLevel(mainWindow)!.StorageProvider;
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = ParseFilter(filter)
        });
        return result.Count == 0 ? null : result[0].Path.AbsolutePath;
    }

    public static void SetMainWindow(MainWindow window)
    {
        _instance = new Dialogs(window);
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