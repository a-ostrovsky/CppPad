#region

using CppPad.Common;
using CppPad.FileSystem;
using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.ScriptFile.Implementation;

public class ScriptLoader(
    DiskFileSystem fileSystem,
    IScriptParser parser,
    ILoggerFactory loggerFactory) : IScriptLoader
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ScriptLoader>();

    public async Task<ScriptDocument> LoadAsync(string path)
    {
        if (!fileSystem.FileExists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        _logger.LogInformation("Loading script from {Path}", path);
        var content = await fileSystem.ReadAllTextAsync(path);
        _logger.LogInformation("Loaded script from {Path}", path);
        var isScriptFile = path.EndsWith(AppConstants.DefaultFileExtension);
        var scriptDto = isScriptFile
            ? parser.Parse(content)
            : parser.FromCppFile(content);

        var (identifier, script) = ScriptConverter.DtoToScript(scriptDto);

        identifier ??= IdGenerator.GenerateUniqueId();
        return new ScriptDocument
        {
            FileName = isScriptFile ? path : null,
            Identifier = identifier,
            Script = script
        };
    }

    public async Task SaveAsync(ScriptDocument scriptDocument)
    {
        if (scriptDocument.FileName == null)
        {
            throw new InvalidOperationException("Cannot save script without a file name.");
        }

        _logger.LogInformation("Saving script to {Path}", scriptDocument.FileName);
        var dto = ScriptConverter.ScriptToDto(scriptDocument.Script, scriptDocument.Identifier);
        var content = parser.Serialize(dto);
        await fileSystem.WriteAllTextAsync(scriptDocument.FileName, content);
        _logger.LogInformation("Saved script to {Path}", scriptDocument.FileName);
    }
}