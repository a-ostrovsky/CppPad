#region

using CppPad.CompilerAdapter.Msvc.Interface;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Impl;

public class VsWhereAdapter(ILoggerFactory loggerFactory)
    : IVsWhereAdapter
{
    private static readonly string VswherePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder
            .ProgramFilesX86),
        @"Microsoft Visual Studio\Installer\vswhere.exe");


    private readonly ILogger _logger =
        loggerFactory.CreateLogger<VsWhereAdapter>();

    public async Task<ICollection<string>> GetVisualStudioPathsAsync()
    {
        if (!File.Exists(VswherePath))
        {
            _logger.LogInformation("vswhere.exe not found");
            return Array.Empty<string>();
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = VswherePath,
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