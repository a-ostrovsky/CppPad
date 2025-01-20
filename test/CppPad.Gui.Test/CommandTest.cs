using CppPad.Gui.Input;

namespace CppPad.Gui.Tests;

public class CommandTest
{
    [Fact]
    public void RelayCommand_CanExecute_ReturnsTrue_WhenCanExecuteIsNull()
    {
        // Arrange
        var command = new RelayCommand(_ => { });

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void RelayCommand_CanExecute_ReturnsFalse_WhenCanExecuteIsFalse()
    {
        // Arrange
        var command = new RelayCommand(_ => { }, _ => false);

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public void RelayCommand_Execute_InvokesAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(_ => { executed = true; });

        // Act
        command.Execute(null);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void AsyncRelayCommand_CanExecute_ReturnsTrue_WhenCanExecuteIsNull()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask);

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.True(canExecute);
    }

    [Fact]
    public void AsyncRelayCommand_CanExecute_ReturnsFalse_WhenCanExecuteIsFalse()
    {
        // Arrange
        var command = new AsyncRelayCommand(_ => Task.CompletedTask, _ => false);

        // Act
        var canExecute = command.CanExecute(null);

        // Assert
        Assert.False(canExecute);
    }

    [Fact]
    public async Task AsyncRelayCommand_ExecuteAsync_InvokesFunc()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Act
        await command.ExecuteAsync(null);

        // Assert
        Assert.True(executed);
    }
}