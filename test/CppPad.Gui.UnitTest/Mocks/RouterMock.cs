#region

using Avalonia.Controls;
using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;
using System.Collections.Concurrent;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class RouterMock : IRouter
{
    private readonly ConcurrentDictionary<Type, object> _viewModelsByType = new();
    private bool _askUserResult = true;
    private object? _inputBoxResult;
    private Uri? _selectedFile = new("file:///c:/script.cpad");

    private readonly List<ViewModelBase> _shownViewModelsOfDialogs = [];

    public bool WasDialogShownForViewModel<T>()
    {
        return _shownViewModelsOfDialogs.Any(x => x.GetType() == typeof(T));
    }

    public Task<T?> ShowDialogAsync<T>() where T : ViewModelBase
    {
        if (!_viewModelsByType.TryGetValue(typeof(T), out var ret))
        {
            throw new InvalidOperationException($"ViewModel of type {typeof(T)} not found.");
        }
        _shownViewModelsOfDialogs.Add((ViewModelBase)ret);

        return Task.FromResult((T)ret)!;
    }

    public Task<T> ShowDialogAsync<T>(T viewModel) where T : ViewModelBase
    {
        _shownViewModelsOfDialogs.Add(viewModel);
        return Task.FromResult(viewModel);
    }

    public Task<Uri?> ShowSaveFileDialogAsync(string filter)
    {
        return Task.FromResult(_selectedFile);
    }

    public Task<Uri?> ShowOpenFileDialogAsync(string filter)
    {
        return Task.FromResult(_selectedFile);
    }

    public Task<bool> AskUserAsync(string title, string text)
    {
        return Task.FromResult(_askUserResult);
    }

    public Task<T?> ShowInputBoxAsync<T>(string prompt)
    {
        return Task.FromResult((T)_inputBoxResult!)!;
    }

    public void SetMainWindow(Window window)
    {
        // No action
    }

    public void SetAskUserResult(bool result)
    {
        _askUserResult = result;
    }

    public void SetInputBoxResult<T>(T result)
    {
        _inputBoxResult = result;
    }

    public void RegisterViewModel<T>(T viewModel) where T : ViewModelBase
    {
        _viewModelsByType[typeof(T)] = viewModel;
    }

    public void SetSelectedFile(Uri? uri)
    {
        _selectedFile = uri;
    }

    public void SetSomeSelectedFile()
    {
        _selectedFile = new Uri("file:///c:/script.cpad");
    }
}