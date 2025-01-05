namespace CppPad.UniqueIdentifier;

public record Identifier(string Value)
{
    public static Identifier Empty { get; } = new(string.Empty);

    public override string ToString()
    {
        return Value;
    }
}
