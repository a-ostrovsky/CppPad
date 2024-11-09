#region

using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CppPad.CompilerAdapter.Interface;

#endregion

namespace CppPad.ScriptFile.Implementation;

public class ScriptParser(ILoggerFactory loggerFactory) : IScriptParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly ILogger _logger = loggerFactory.CreateLogger<ScriptParser>();

    public Script Parse(string content)
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

            var script = new Script
            {
                Content = scriptDto.Content,
                AdditionalIncludeDirs = scriptDto.AdditionalIncludeDirs,
                LibrarySearchPaths = scriptDto.LibrarySearchPaths,
                AdditionalEnvironmentPaths = scriptDto.AdditionalEnvironmentPaths,
                StaticallyLinkedLibraries = scriptDto.StaticallyLinkedLibraries,
                CppStandard = Enum.Parse<CppStandard>(scriptDto.CppStandard),
                OptimizationLevel = Enum.Parse<OptimizationLevel>(scriptDto.OptimizationLevel),
                AdditionalBuildArgs = scriptDto.AdditionalBuildArgs,
                PreBuildCommand = scriptDto.PreBuildCommand
            };
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
            Content = content
        };
        return script;
    }

    public string Serialize(Script script)
    {
        try
        {
            var scriptDto = new ScriptDto
            {
                Content = script.Content,
                AdditionalIncludeDirs = script.AdditionalIncludeDirs,
                LibrarySearchPaths = script.LibrarySearchPaths,
                AdditionalEnvironmentPaths = script.AdditionalEnvironmentPaths,
                StaticallyLinkedLibraries = script.StaticallyLinkedLibraries,
                CppStandard = script.CppStandard.ToString(),
                OptimizationLevel = script.OptimizationLevel.ToString(),
                AdditionalBuildArgs = script.AdditionalBuildArgs,
                PreBuildCommand = script.PreBuildCommand
            };
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