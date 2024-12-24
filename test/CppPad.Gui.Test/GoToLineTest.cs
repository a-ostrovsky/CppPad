namespace CppPad.Gui.Tests;

public class GoToLineTest
{
    private readonly Bootstrapper _bootstrapper = new();
    private readonly FakeDialogs _dialogs = FakeDialogs.Use();

    [Fact]
    public void GoToLine_ValidInput_ChangesCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        _dialogs.WillReturnInputBoxResponse("2:3");

        // Act
        _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.Execute(null);

        // Assert
        Assert.Equal(2, editor.SourceCode.CurrentLine);
        Assert.Equal(3, editor.SourceCode.CurrentColumn);
    }

    [Fact]
    public void GoToLine_InvalidLineNumber_DoesNotChangeCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        editor.SourceCode.CurrentLine = 1;
        editor.SourceCode.CurrentColumn = 1;
        _dialogs.WillReturnInputBoxResponse("5:3");

        // Act
        _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.Execute(null);

        // Assert
        Assert.Equal(1, editor.SourceCode.CurrentLine);
        Assert.Equal(1, editor.SourceCode.CurrentColumn);
    }

    [Fact]
    public void GoToLine_InvalidColumnNumber_SetsColumnToEndOfLine()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        _dialogs.WillReturnInputBoxResponse("2:10");

        // Act
        _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.Execute(null);

        // Assert
        Assert.Equal(2, editor.SourceCode.CurrentLine);
        Assert.Equal(6, editor.SourceCode.CurrentColumn); // "Line 2" has 6 characters
    }

    [Fact]
    public void GoToLine_EmptyInput_DoesNotChangeCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        editor.SourceCode.CurrentLine = 1;
        editor.SourceCode.CurrentColumn = 1;
        _dialogs.WillReturnInputBoxResponse("");

        // Act
        _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.Execute(null);

        // Assert
        Assert.Equal(1, editor.SourceCode.CurrentLine);
        Assert.Equal(1, editor.SourceCode.CurrentColumn);
    }

    [Fact]
    public void GoToLine_CancelledInput_DoesNotChangeCaretPosition()
    {
        // Arrange
        var editor = _bootstrapper.OpenEditorsViewModel.CurrentEditor!;
        editor.SourceCode.Content = "Line 1\nLine 2\nLine 3";
        editor.SourceCode.CurrentLine = 1;
        editor.SourceCode.CurrentColumn = 1;
        _dialogs.WillReturnInputBoxResponse(null);

        // Act
        _bootstrapper.MainWindowViewModel.Toolbar.GoToLineCommand.Execute(null);

        // Assert
        Assert.Equal(1, editor.SourceCode.CurrentLine);
        Assert.Equal(1, editor.SourceCode.CurrentColumn);
    }
}