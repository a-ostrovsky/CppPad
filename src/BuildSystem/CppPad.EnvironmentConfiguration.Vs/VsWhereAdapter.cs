using System.Text;
using CppPad.Logging;
using CppPad.SystemAdapter.Execution;
using Microsoft.Extensions.Logging;

namespace CppPad.EnvironmentConfiguration.Vs;

public class VsWhereAdapter(Process process, CancellationToken token = default)
    : IVsWhereAdapter
{
    private readonly ILogger _logger = Log.CreateLogger<VsWhereAdapter>();
    
    private static readonly string VsWherePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder
            .ProgramFilesX86),
        @"Microsoft Visual Studio\Installer\vswhere.exe");

    public async Task<ICollection<string>> GetVisualStudioPathsAsync()
    {
        if (!File.Exists(VsWherePath))
        {
            throw new EnvironmentConfigurationException(
                "Visual Studio is not installed.");
        }

        var errorBuilder = new StringBuilder();
        var resultBuilder = new StringBuilder();

        var processInfo = process.Start(new StartInfo
        {
            FileName = VsWherePath,
            Arguments =
            [
                "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath"
            ],
            ErrorReceived = (_, args) => errorBuilder.Append(args.Data),
            OutputReceived = (_, args) => resultBuilder.Append(args.Data)
        });
        await process.WaitForExitAsync(processInfo, token);
        if (errorBuilder.Length > 0)
        {
            throw new EnvironmentConfigurationException(
                $"Failed to get Visual Studio paths: {errorBuilder}");
        }

        var result = resultBuilder
            .ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        _logger.LogDebug("Found Visual Studio installations: {result}", result.ToArray<object?>());
        return result;
    }
}