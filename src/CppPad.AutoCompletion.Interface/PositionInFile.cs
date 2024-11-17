namespace CppPad.AutoCompletion.Interface;

public record PositionInFile
{
    public required string FileName { get; init; }
    
    public required Position Position { get; init; }
}