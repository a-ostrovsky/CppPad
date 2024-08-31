#region

using System.Diagnostics;

using CppPad.CompilerAdapter.Interface;

using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.CompilerAdapter.Msvc
{
    public class ToolsetDetector(ILoggerFactory loggerFactory)
        : IToolsetDetector
    {
        private readonly ILogger<ToolsetDetector> _logger =
            loggerFactory.CreateLogger<ToolsetDetector>();

        private readonly string _vswherePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder
                .ProgramFilesX86),
            @"Microsoft Visual Studio\Installer\vswhere.exe");

        public async Task<ICollection<Toolset>> GetToolsetsAsync()
        {
            var visualStudioPaths = await GetVisualStudioPathsAsync();
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
            var combinations = new List<(string, string)>
            {
                ("Hostx64", "x64"),
                ("Hostx64", "x86"),
                ("Hostx86", "x64"),
                ("Hostx86", "x86")
            };
            foreach (var (host, target) in combinations)
            {
                var executablePath =
                    Path.Combine(binPath, @$"{host}\{target}\cl.exe");
                if (File.Exists(executablePath))
                {
                    result.Add(new Toolset($"MSVC {version}",
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
            if (!File.Exists(versionsFilePath))
            {
                _logger.LogInformation(
                    "Microsoft.VCToolsVersion.default.txt not found in '{visualStudioPath}'",
                    versionsFilePath);
            }

            var lines = await File.ReadAllLinesAsync(versionsFilePath);
            return lines;
        }

        private async Task<ICollection<string>> GetVisualStudioPathsAsync()
        {
            if (!File.Exists(_vswherePath))
            {
                _logger.LogInformation("vswhere.exe not found");
                return Array.Empty<string>();
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _vswherePath,
                    Arguments =
                        "-products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            var visualStudioPaths = output.Split(Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries);
            return visualStudioPaths;
        }
    }
}
