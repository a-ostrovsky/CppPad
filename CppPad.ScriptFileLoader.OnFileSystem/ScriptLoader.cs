#region

using CppPad.Common;
using CppPad.FileSystem;
using CppPad.ScriptFile.Interface;
using CppPad.ScriptFileLoader.Interface;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.ScriptFileLoader.OnFileSystem;

public class ScriptLoader(
    DiskFileSystem fileSystem,
    IScriptParser parser,
    ILoggerFactory loggerFactory) : IScriptLoader
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ScriptLoader>();

    public async Task<Script> LoadAsync(string path)
    {
        if (!fileSystem.FileExists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        _logger.LogInformation("Loading script from {Path}", path);
        var content = await fileSystem.ReadAllTextAsync(path);
        _logger.LogInformation("Loaded script from {Path}", path);
        return path.EndsWith(AppConstants.DefaultFileExtension)
            ? parser.Parse(content)
            : parser.FromCppFile(content);
    }

    public async Task SaveAsync(string path, Script script)
    {
        _logger.LogInformation("Saving script to {Path}", path);
        var content = parser.Serialize(script);
        await fileSystem.WriteAllTextAsync(path, content);
        _logger.LogInformation("Saved script to {Path}", path);
    }
}