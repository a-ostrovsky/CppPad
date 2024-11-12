namespace CppPad.AutoCompletion.Interface;

public record Position
{
    public required int Line { get; init; }
    public required int Character { get; init; }
}