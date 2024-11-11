namespace CppPad.Common;

public record Identifier(string Value)
{
    public static Identifier Empty { get; } = new Identifier(string.Empty);

    public override string ToString()
    {
        return Value;
    }
}
