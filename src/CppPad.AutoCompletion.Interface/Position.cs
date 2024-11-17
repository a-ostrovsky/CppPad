namespace CppPad.AutoCompletion.Interface;

public record Position
{
    /// <summary>
    /// Gets the zero based line number.
    /// </summary>
    public required int Line { get; init; }
    
    /// <summary>
    /// Gets the zero based character index in the line.
    /// </summary>
    public required int Character { get; init; }
}