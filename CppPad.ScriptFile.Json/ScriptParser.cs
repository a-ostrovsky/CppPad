using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CppPad.ScriptFile.Json;

public class ScriptParser(ILoggerFactory loggerFactory) : IScriptParser
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ScriptParser>();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public Script Parse(string content)
    {
        _logger.LogInformation("Parsing script content.");
        try
        {
            var script = JsonSerializer.Deserialize<Script>(content, SerializerOptions);
            if (script == null)
            {
                throw new ParsingException("Failed to parse script content.");
            }

            if (script.Version != 1)
            {
                throw new ParsingException($"Unsupported script version: {script.Version}.");
            }
            return script;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing script content.");
            throw new ParsingException("Error parsing script content.", ex);
        }
    }

    public Script FromCppFile(string content)
    {
        _logger.LogInformation("Creating Script object from C++ file content.");
        var script = new Script
        {
            Content = content,
        };
        return script;
    }

    public string Serialize(Script script)
    {
        try
        {
            var json = JsonSerializer.Serialize(script, SerializerOptions);
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing Script object.");
            throw;
        }
    }
}