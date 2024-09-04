#region

using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.MockFileSystem;
using CppPad.ScriptFile.Interface;
using CppPad.ScriptFile.Json;
using DeepEqual.Syntax;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.ScriptFileLoader.OnFileSystem.Test;

public class TemplateLoaderTest
{
    private readonly InMemoryFileSystem _fileSystem = new();
    private readonly TemplateLoader _templateLoader;

    public TemplateLoaderTest()
    {
        _templateLoader = new TemplateLoader(_fileSystem,
            new ScriptParser(NullLoggerFactory.Instance), NullLoggerFactory.Instance);
        _fileSystem.AlwaysCreateDirectoriesIfNotExist();
    }

    [Fact]
    public async Task LoadAsync_TemplateDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        const string templateName = "nonexistentTemplate";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _templateLoader.LoadAsync(templateName));
    }

    [Fact]
    public async Task SaveAsyncAndLoadAsync_ValidTemplate_CanLoadSavedTemplate()
    {
        // Arrange
        const string templateName = "testTemplate";
        var script = new Script
        {
            Content = "int main() { return 0; }",
            AdditionalIncludeDirs = ["X:\\Includes"],
            CppStandard = CppStandard.Cpp11,
            OptimizationLevel = OptimizationLevel.Level1,
            AdditionalBuildArgs = "Args1"
        };

        // Act
        await _templateLoader.SaveAsync(templateName, script);
        var loadedScript = await _templateLoader.LoadAsync(templateName);

        // Assert
        loadedScript.ShouldDeepEqual(script);
    }

    [Fact]
    public async Task Delete_TemplateExists_DeletesTemplate()
    {
        // Arrange
        const string templateName = "testTemplate";
        var script1 = new Script();

        await _templateLoader.SaveAsync(templateName, script1);

        // Act
        _templateLoader.Delete(templateName);

        // Assert
        var savedTemplates = await _fileSystem.ListFilesAsync(AppConstants.TemplateFolder);
        Assert.Empty(savedTemplates);
    }

    [Fact]
    public async Task GetAllTemplatesAsync_TemplatesExist_ReturnsTemplateNames()
    {
        // Arrange
        const string templateName1 = "template1";
        const string templateName2 = "template2";
        var script1 = new Script();
        var script2 = new Script();

        await _templateLoader.SaveAsync(templateName1, script1);
        await _templateLoader.SaveAsync(templateName2, script2);

        // Act
        var templates = await _templateLoader.GetAllTemplatesAsync();

        // Assert
        Assert.Contains(templateName1, templates);
        Assert.Contains(templateName2, templates);
    }
}