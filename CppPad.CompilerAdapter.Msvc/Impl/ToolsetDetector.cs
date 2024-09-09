#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Interface;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Impl;

public class ToolsetDetector(
    DiskFileSystem fileSystem,
    IVsWhereAdapter vsWhereAdapter,
    ILoggerFactory loggerFactory)
    : IToolsetDetector
{
    private readonly ILogger<ToolsetDetector> _logger =
        loggerFactory.CreateLogger<ToolsetDetector>();

    public async Task<ICollection<Toolset>> GetToolsetsAsync()
    {
        var visualStudioPaths = await vsWhereAdapter.GetVisualStudioPathsAsync();
        foreach (var path in visualStudioPaths)
        {
            _logger.LogInformation("VS path: {path}", path);
        }

        var toolsets = new List<Toolset>();
        foreach (var visualStudioPath in visualStudioPaths)
        {
            var versions = await GetInstalledVcVersions(visualStudioPath);
            foreach (var version in versions)
            {
                _logger.LogInformation(
                    "VS path: '{visualStudioPath}'. Toolset version: {version}",
                    visualStudioPath, version);
                var toolsetsForVersion =
                    GetToolsets(visualStudioPath, version);
                toolsets.AddRange(toolsetsForVersion);
            }
        }

        return toolsets;
    }

    private List<Toolset> GetToolsets(string visualStudioPath,
        string version)
    {
        var binPath = Path.Combine(visualStudioPath,
            @$"VC\Tools\MSVC\{version}\bin");
        var result = new List<Toolset>();
        var combinations = new List<(string, string, CpuArchitecture)>
        {
            ("Hostx64", "x64", CpuArchitecture.X64),
            ("Hostx64", "x86", CpuArchitecture.X86),
            ("Hostx86", "x64", CpuArchitecture.X64),
            ("Hostx86", "x86", CpuArchitecture.X86)
        };
        foreach (var (host, target, arch) in combinations)
        {
            var executablePath =
                Path.Combine(binPath, @$"{host}\{target}\cl.exe");
            if (fileSystem.FileExists(executablePath))
            {
                result.Add(new Toolset($"MSVC {version}", arch,
                    $"{version} ({host} -> {target})", executablePath));
            }
            else
            {
                _logger.LogInformation(
                    "cl.exe not found in '{executablePath}'",
                    executablePath);
            }
        }

        return result;
    }

    private async Task<ICollection<string>> GetInstalledVcVersions(
        string visualStudioPath)
    {
        var versionsFilePath = Path.Combine(visualStudioPath,
            @"VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt");
        if (!fileSystem.FileExists(versionsFilePath))
        {
            _logger.LogInformation(
                "Microsoft.VCToolsVersion.default.txt not found in '{visualStudioPath}'",
                versionsFilePath);
            return [];
        }

        var lines = await fileSystem.ReadAllLinesAsync(versionsFilePath);
        return lines;
    }
}