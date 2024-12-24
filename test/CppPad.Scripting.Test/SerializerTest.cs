using CppPad.Scripting.Serialization;
using CppPad.UniqueIdentifier;
using DeepEqual.Syntax;

namespace CppPad.Scripting.Test;

public class SerializerTest
{
    private readonly ScriptSerializer _serializer = new();

    [Fact]
    public void SerializeAndDeserialize_ShouldReturnSameDocument()
    {
        // Arrange
        var originalDocument = new ScriptDocument
        {
            Script = new ScriptData
            {
                Content = "int main() { return 0; }",
                BuildSettings = new CppBuildSettings
                {
                    OptimizationLevel = OptimizationLevel.O2,
                    CppStandard = CppStandard.Cpp17
                }
            },
            Identifier = new Identifier("12345"),
            FileName = "test.cpp"
        };

        // Act
        var json = _serializer.Serialize(originalDocument);
        var deserializedDocument = _serializer.Deserialize(json);

        // Assert
        originalDocument.ShouldDeepEqual(deserializedDocument);
    }
    
    [Fact]
    public void Serialize_enum_as_strings()
    {
        // Arrange
        var originalDocument = new ScriptDocument
        {
            Script = new ScriptData
            {
                BuildSettings = new CppBuildSettings
                {
                    CppStandard = CppStandard.Cpp17
                }
            },
            Identifier = new Identifier("12345"),
        };

        // Act
        var json = _serializer.Serialize(originalDocument);

        // Assert
        Assert.Contains("Cpp17", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrowException()
    {
        // Arrange
        const string invalidJson = "xxx";

        // Act & Assert
        Assert.Throws<ScriptSerializationException>(() => _serializer.Deserialize(invalidJson));
    }
}