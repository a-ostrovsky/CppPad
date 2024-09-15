namespace CppPad.Common.UnitTest;

public class ClonerTest
{
    [Fact]
    public void Clone_NullObject_ReturnsNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = Cloner.DeepCopy(obj);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Clone_SimpleObject_ReturnsDeepCopy()
    {
        // Arrange
        var original = new TestClass { Id = 1, Name = "Test" };

        // Act
        var clone = Cloner.DeepCopy(original);

        // Assert
        Assert.NotNull(clone);
        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.Name, clone.Name);
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void Clone_ComplexObject_ReturnsDeepCopy()
    {
        // Arrange
        var original = new TestClass
        {
            Id = 1,
            Name = "Test",
            Nested = new NestedClass { Description = "Nested" }
        };

        // Act
        var clone = Cloner.DeepCopy(original);

        // Assert
        Assert.NotNull(clone);
        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.Name, clone.Name);
        Assert.NotSame(original, clone);
        Assert.NotNull(clone.Nested);
        Assert.Equal(original.Nested?.Description, clone.Nested?.Description);
        Assert.NotSame(original.Nested, clone.Nested);
    }

    private class TestClass
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public NestedClass? Nested { get; init; }
    }

    private class NestedClass
    {
        public string? Description { get; init; }
    }
}