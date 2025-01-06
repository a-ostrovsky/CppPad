namespace CppPad.LspClient.Model;

public record SourceCodeRange
{
    /// <summary>
    ///     Gets the zero based inclusive start position of the range.
    /// </summary>
    public required Position Start { get; init; }

    /// <summary>
    ///     Gets the zero based inclusive end position of the range.
    /// </summary>
    public required Position End { get; init; }
}
