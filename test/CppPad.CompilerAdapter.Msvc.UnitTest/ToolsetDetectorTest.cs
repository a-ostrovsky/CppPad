#region

using CppPad.CompilerAdapter.Msvc.Impl;
using CppPad.CompilerAdapter.Msvc.UnitTest.Mocks;
using CppPad.MockFileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.CompilerAdapter.Msvc.UnitTest;

public class ToolsetDetectorTest
{
    private readonly ToolsetDetector _detector;
    private readonly InMemoryFileSystem _fileSystem = new();
    private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
    private readonly VsWhereAdapterMock _vsWhereAdapterMock = new();

    public ToolsetDetectorTest()
    {
        _fileSystem.AlwaysCreateDirectoriesIfNotExist();
        _detector = new ToolsetDetector(_fileSystem, _vsWhereAdapterMock, _loggerFactory);
    }

    [Fact]
    public async Task GetToolsetsAsync_NoVisualStudioInstalled_ReturnsEmpty()
    {
        // Act
        var toolsets = await _detector.GetToolsetsAsync();

        // Assert
        Assert.Empty(toolsets);
    }

    [Fact]
    public async Task GetToolsetsAsync_VisualStudioInstalledWithoutMsvc_ReturnsEmpty()
    {
        // Arrange
        _vsWhereAdapterMock.SetVisualStudioPaths("C:\\VSWithoutVcTools");

        // Act
        var toolsets = await _detector.GetToolsetsAsync();

        // Assert
        Assert.Empty(toolsets);
    }

    [Fact]
    public async Task GetToolsetsAsync_VisualStudioInstalledWithSingleMsvcVersion_ReturnsToolsets()
    {
        // Arrange
        const string vsPath = "C:\\VSWithVcTools";
        _vsWhereAdapterMock.SetVisualStudioPaths(vsPath);
        await _fileSystem.WriteAllTextAsync(
            Path.Combine(vsPath, @"VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt"),
            "14.29.30037");
        await _fileSystem.CreateDirectoryAsync(Path.Combine(vsPath,
            @"VC\Tools\MSVC\14.29.30037\bin\Hostx64\x64"));
        await _fileSystem.WriteAllTextAsync(
            Path.Combine(vsPath, @"VC\Tools\MSVC\14.29.30037\bin\Hostx64\x64\cl.exe"),
            string.Empty);

        // Act
        var toolsets = await _detector.GetToolsetsAsync();

        // Assert
        Assert.NotEmpty(toolsets);
        Assert.Contains(toolsets, t => t.Name.Contains("14.29.30037"));
    }

    [Fact]
    public async Task GetToolsetsAsync_VisualStudioPathWithMultipleMsvcVersions_ReturnsAllToolsets()
    {
        // Arrange
        const string vsPath = "C:\\VSWithMultipleVcTools";
        _vsWhereAdapterMock.SetVisualStudioPaths(vsPath);
        await _fileSystem.WriteAllTextAsync(
            Path.Combine(vsPath, @"VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt"),
            "14.29.30037" + Environment.NewLine + "14.28.29910");
        await _fileSystem.CreateDirectoryAsync(Path.Combine(vsPath,
            @"VC\Tools\MSVC\14.29.30037\bin\Hostx64\x64"));
        await _fileSystem.WriteAllTextAsync(
            Path.Combine(vsPath, @"VC\Tools\MSVC\14.29.30037\bin\Hostx64\x64\cl.exe"),
            string.Empty);
        await _fileSystem.CreateDirectoryAsync(Path.Combine(vsPath,
            @"VC\Tools\MSVC\14.28.29910\bin\Hostx64\x64"));
        await _fileSystem.WriteAllTextAsync(
            Path.Combine(vsPath, @"VC\Tools\MSVC\14.28.29910\bin\Hostx64\x64\cl.exe"),
            string.Empty);

        // Act
        var toolsets = await _detector.GetToolsetsAsync();

        // Assert
        Assert.Equal(2, toolsets.Count);
        Assert.Contains(toolsets, t => t.Name.Contains("14.29.30037"));
        Assert.Contains(toolsets, t => t.Name.Contains("14.28.29910 (Hostx64 -> x64)"));
    }

    [Fact]
    public async Task GetToolsetsAsync_VisualStudioPathWithInvalidVcToolsVersionFile_ReturnsEmpty()
    {
        // Arrange
        const string vsPath = "C:\\VSWithInvalidVcTools";
        _vsWhereAdapterMock.SetVisualStudioPaths(vsPath);
        await _fileSystem.WriteAllTextAsync(
            Path.Combine(vsPath, @"VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt"),
            "invalid_version");

        // Act
        var toolsets = await _detector.GetToolsetsAsync();

        // Assert
        Assert.Empty(toolsets);
    }
}