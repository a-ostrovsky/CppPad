#region

using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.ScriptFile.Json.UnitTest;

public class ScriptParserTest
{
    private readonly ScriptParser _scriptParser;

    public ScriptParserTest()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        _scriptParser = new ScriptParser(loggerFactory);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsException()
    {
        // Arrange
        const string invalidJsonContent = "invalid json";

        // Act & Assert
        Assert.Throws<ParsingException>(() => _scriptParser.Parse(invalidJsonContent));
    }

    [Fact]
    public void SerializeAndDeserialize_ValidScript_ReturnsSameScript()
    {
        // Arrange
        var script = new Script { Content = "test content" };

        // Act
        var json = _scriptParser.Serialize(script);
        var deserializedScript = _scriptParser.Parse(json);

        // Assert
        Assert.NotNull(json);
        Assert.NotNull(deserializedScript);
        Assert.Equal(script.Content, deserializedScript.Content);
    }

    [Fact]
    public void SerializeAndDeserialize_UnsupportedVersion_ThrowsException()
    {
        // Arrange
        var script = new Script { Version = 2 };
        var json = _scriptParser.Serialize(script);

        // Act & Assert
        Assert.Throws<ParsingException>(() => _scriptParser.Parse(json));
    }

    [Fact]
    public void FromCppFile_ValidContent_ReturnsScript()
    {
        // Arrange
        const string cppContent = "int main() { return 0; }";

        // Act
        var result = _scriptParser.FromCppFile(cppContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cppContent, result.Content);
    }
}