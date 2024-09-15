#region

using Avalonia.Controls;
using CppPad.Gui.ViewModels;
using System;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.Routing;

public interface IRouter
{
    Task<T?> ShowDialogAsync<T>() where T : ViewModelBase;

    Task<T> ShowDialogAsync<T>(T viewModel) where T : ViewModelBase;

    Task<Uri?> ShowSaveFileDialogAsync(string filter);

    Task<Uri?> ShowOpenFileDialogAsync(string filter);

    Task<bool> AskUserAsync(string title, string text);

    Task<T?> ShowInputBoxAsync<T>(string prompt);

    void SetMainWindow(Window window);
}

public class DummyRouter : IRouter
{
    public Task<T?> ShowDialogAsync<T>() where T : ViewModelBase
    {
        return Task.FromResult(default(T));
    }

    public Task<T> ShowDialogAsync<T>(T viewModel) where T : ViewModelBase
    {
        return Task.FromResult(viewModel);
    }

    public Task<bool> AskUserAsync(string title, string text)
    {
        return Task.FromResult(false);
    }

    public Task<T?> ShowInputBoxAsync<T>(string prompt)
    {
        return Task.FromResult(default(T));
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