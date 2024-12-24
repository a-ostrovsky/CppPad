#region

using System;
using System.Windows.Input;

#endregion

namespace CppPad.Gui;

public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return canExecute == null || canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}