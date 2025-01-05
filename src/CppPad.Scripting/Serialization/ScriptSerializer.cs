using System.Text.Json;
using System.Text.Json.Serialization;
using CppPad.Scripting.Serialization.Dtos;
using CppPad.UniqueIdentifier;

namespace CppPad.Scripting.Serialization;

[JsonSerializable(typeof(ScriptDocumentDto))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
internal partial class ScriptJsonContext : JsonSerializerContext { }

public class ScriptSerializer
{
    public string Serialize(ScriptDocument script)
    {
        var dto = new ScriptDocumentDto
        {
            Script = new ScriptDataDto
            {
                Content = script.Script.Content,
                BuildSettings = new CppBuildSettingsDto
                {
                    OptimizationLevel = script.Script.BuildSettings.OptimizationLevel,
                    CppStandard = script.Script.BuildSettings.CppStandard,
                },
            },
            Identifier = script.Identifier.ToString(),
            FileName = script.FileName,
        };

        return JsonSerializer.Serialize(dto, ScriptJsonContext.Default.ScriptDocumentDto);
    }

    public ScriptDocument Deserialize(string json)
    {
        ScriptDocumentDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<ScriptDocumentDto>(
                json,
                ScriptJsonContext.Default.ScriptDocumentDto
            );
        }
        catch (JsonException e)
        {
            throw new ScriptSerializationException(
                "Failed to deserialize script document. Invalid json.",
                e
            );
        }

        if (dto == null)
        {
            throw new ScriptSerializationException("Failed to deserialize script document.");
        }

        return new ScriptDocument
        {
            Script = new ScriptData
            {
                Content = dto.Script.Content,
                BuildSettings = new CppBuildSettings
                {
                    OptimizationLevel = dto.Script.BuildSettings.OptimizationLevel,
                    CppStandard = dto.Script.BuildSettings.CppStandard,
                },
            },
            Identifier = new Identifier(dto.Identifier),
            FileName = dto.FileName,
        };
    }
}
