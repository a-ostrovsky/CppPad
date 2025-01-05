#region

using System;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.Input;

public class AsyncRelayCommand(Func<object?, Task?> execute, Predicate<object?>? canExecute = null)
    : IAsyncCommand
{
    public event EventHandler? CanExecuteChanged;

    public Task ExecuteAsync(object? parameter)
    {
        return execute(parameter) ?? Task.CompletedTask;
    }

    public bool CanExecute(object? parameter)
    {
        return canExecute == null || canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _ = execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
