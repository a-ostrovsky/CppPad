namespace CppPad.UniqueIdentifier.Test;

public class IdentifierTest
{
    [Fact]
    public void Identifier_ShouldReturnEmptyIdentifier()
    {
        var result = Identifier.Empty;
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void Identifier_ToString_ShouldReturnValue()
    {
        const string value = "test";
        var identifier = new Identifier(value);
        var result = identifier.ToString();
        Assert.Equal(value, result);
    }
}