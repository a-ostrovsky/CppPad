#region

using Avalonia.Controls;
using CppPad.Gui.ViewModels;
using System;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.Routing;

public interface IRouter
{
    Task ShowDialogAsync<T>() where T : ViewModelBase;

    Task<Uri?> ShowSaveFileDialogAsync(string filter);

    Task<Uri?> ShowOpenFileDialogAsync(string filter);

    Task<bool> AskUserAsync(string title, string text);

    void SetMainWindow(Window window);
}

public class DummyRouter : IRouter
{
    public Task ShowDialogAsync<T>() where T : ViewModelBase
    {
        return Task.CompletedTask;
    }

    public Task<bool> AskUserAsync(string title, string text)
    {
        return Task.FromResult(false);
    }

    public void SetMainWindow(Window window)
    {
        //No action
    }

    public Task<Uri?> ShowSaveFileDialogAsync(string filter)
    {
        return Task.FromResult<Uri?>(new Uri("localhost"));
    }

    public Task<Uri?> ShowOpenFileDialogAsync(string filter)
    {
        return Task.FromResult<Uri?>(new Uri("localhost"));
    }
}