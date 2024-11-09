#region

using System.Text.Json;
using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.ScriptFile.Implementation;

public class ScriptParser(ILoggerFactory loggerFactory) : IScriptParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly ILogger _logger = loggerFactory.CreateLogger<ScriptParser>();

    public ScriptDto Parse(string content)
    {
        _logger.LogInformation("Parsing script content.");
        try
        {
            var scriptDto = JsonSerializer.Deserialize<ScriptDto>(content, SerializerOptions);
            if (scriptDto == null)
            {
                throw new ParsingException("Failed to parse script content.");
            }

            if (scriptDto.Version != 1)
            {
                throw new ParsingException($"Unsupported script version: {scriptDto.Version}.");
            }


            return scriptDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing script content.");
            throw new ParsingException("Error parsing script content.", ex);
        }
    }

    public ScriptDto FromCppFile(string content)
    {
        _logger.LogInformation("Creating Script object from C++ file content.");
        var script = new ScriptDto
        {
            Content = content
        };
        return script;
    }

    public string Serialize(ScriptDto scriptDto)
    {
        try
        {
            var json = JsonSerializer.Serialize(scriptDto, SerializerOptions);
            return json;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing Script object.");
            throw;
        }
    }
}