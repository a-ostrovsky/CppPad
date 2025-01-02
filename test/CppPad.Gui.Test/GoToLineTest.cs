namespace CppPad.Gui.Tests;

public class GoToLineTest : IDisposable
{
    private readonly Bootstrapper _bootstrapper = new();
    
    public void Dispose()
    {
        _bootstrapper.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GoToLine_ValidInput_ChangesCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        _bootstrapper.Dialogs.WillReturnInputBoxResponse("2:3");

        // Act
        await _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, editor.SourceCode.CurrentLine);
        Assert.Equal(3, editor.SourceCode.CurrentColumn);
    }

    [Fact]
    public async Task GoToLine_InvalidLineNumber_DoesNotChangeCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        editor.SourceCode.CurrentLine = 1;
        editor.SourceCode.CurrentColumn = 1;
        _bootstrapper.Dialogs.WillReturnInputBoxResponse("5:3");

        // Act
        await _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, editor.SourceCode.CurrentLine);
        Assert.Equal(1, editor.SourceCode.CurrentColumn);
    }

    [Fact]
    public async Task GoToLine_InvalidColumnNumber_SetsColumnToEndOfLine()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        _bootstrapper.Dialogs.WillReturnInputBoxResponse("2:10");

        // Act
        await _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, editor.SourceCode.CurrentLine);
        Assert.Equal(6, editor.SourceCode.CurrentColumn); // "Line 2" has 6 characters
    }

    [Fact]
    public async Task GoToLine_EmptyInput_DoesNotChangeCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        editor.SourceCode.CurrentLine = 1;
        editor.SourceCode.CurrentColumn = 1;
        _bootstrapper.Dialogs.WillReturnInputBoxResponse("");

        // Act
        await _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, editor.SourceCode.CurrentLine);
        Assert.Equal(1, editor.SourceCode.CurrentColumn);
    }

    [Fact]
    public async Task GoToLine_CancelledInput_DoesNotChangeCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        editor.SourceCode.CurrentLine = 1;
        editor.SourceCode.CurrentColumn = 1;
        _bootstrapper.Dialogs.WillReturnInputBoxResponse(null);

        // Act
        await _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, editor.SourceCode.CurrentLine);
        Assert.Equal(1, editor.SourceCode.CurrentColumn);
    }
}