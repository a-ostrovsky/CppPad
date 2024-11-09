#region

using CppPad.Common;
using CppPad.FileSystem;
using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.ScriptFile.Implementation;

public class TemplateLoader(
    DiskFileSystem fileSystem,
    IScriptParser parser,
    ILoggerFactory loggerFactory) : ITemplateLoader
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TemplateLoader>();

    public event EventHandler<EventArgs>? TemplatesChanged;

    public async Task<Script> LoadAsync(string templateName)
    {
        var fileName = GetTemplateFileName(templateName);
        if (!fileSystem.FileExists(fileName))
        {
            throw new FileNotFoundException($"Template file not found: {fileName}.");
        }

        _logger.LogInformation("Loading template {name}. File name: {fileName}", templateName,
            fileName);
        var content = await fileSystem.ReadAllTextAsync(fileName);
        _logger.LogInformation("Loaded template {fileName}.", fileName);
        var dto = parser.Parse(content);
        var (_, script) = ScriptConverter.DtoToScript(dto);
        return script;
    }

    public async Task SaveAsync(string templateName, Script template)
    {
        var fileName = GetTemplateFileName(templateName);
        _logger.LogInformation("Saving template {name}. File name: {fileName}", templateName,
            fileName);
        var dto = ScriptConverter.ScriptToDto(template);
        var content = parser.Serialize(dto);
        await fileSystem.WriteAllTextAsync(fileName, content);
        _logger.LogInformation("Saved template to {fileName}", fileName);
        TemplatesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Delete(string templateName)
    {
        var fileName = GetTemplateFileName(templateName);
        _logger.LogInformation("Deleting template {name}. File name: {fileName}", templateName,
            fileName);
        fileSystem.DeleteFile(fileName);
        _logger.LogInformation("Deleted template {fileName}.", fileName);
        TemplatesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<IReadOnlyList<string>> GetAllTemplatesAsync()
    {
        _logger.LogInformation("Getting all templates");
        if (!fileSystem.DirectoryExists(AppConstants.TemplateFolder))
        {
            return [];
        }

        const string pattern = $"*{AppConstants.DefaultFileExtension}";
        var files = await fileSystem.ListFilesAsync(AppConstants.TemplateFolder, pattern);
        var names = files.Select(Path.GetFileNameWithoutExtension).OfType<string>().ToList();
        _logger.LogInformation("Got all templates");
        return names;
    }

    private static string GetTemplateFileName(string templateName)
    {
        return Path.Combine(AppConstants.TemplateFolder,
            $"{templateName}{AppConstants.DefaultFileExtension}");
    }
}