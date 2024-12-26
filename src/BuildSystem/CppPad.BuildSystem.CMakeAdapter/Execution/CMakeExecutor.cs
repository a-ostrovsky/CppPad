using CppPad.SystemAdapter.Execution;
using CppPad.SystemAdapter.IO;

namespace CppPad.BuildSystem.CMakeAdapter.Execution;

public class CMakeExecutor(DiskFileSystem fileSystem, Process process)
{
    public async Task RunCMakeAsync(
        CMakeExecutionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!fileSystem.DirectoryExists(options.BuildDirectory))
        {
            await fileSystem.CreateDirectoryAsync(options.BuildDirectory);
        }

        await ConfigureAsync(options, cancellationToken);
        await BuildAsync(options, cancellationToken);
    }

    private async Task ConfigureAsync(CMakeExecutionOptions options, CancellationToken cancellationToken = default)
    {
        if (!options.ForceConfigure)
        {
            var configureMarkerFile = Path.Combine(options.BuildDirectory, "CMakeCache.txt");
            if (fileSystem.FileExists(configureMarkerFile))
            {
                return;
            }
        }

        var startInfo = CreateStartInfoForConfigure(options);
        var processInfo = process.Start(startInfo);
        var errorCode = await process.WaitForExitAsync(processInfo, cancellationToken);
        if (errorCode != 0)
        {
            throw new CMakeExecutionException(
                $"CMake configure failed with error code {errorCode}.");
        }
    }

    private async Task BuildAsync(CMakeExecutionOptions options, CancellationToken cancellationToken = default)
    {
        var startInfo = CreateStartInfoForBuild(options);
        var processInfo = process.Start(startInfo);
        var errorCode = await process.WaitForExitAsync(processInfo, cancellationToken);
        if (errorCode != 0)
        {
            throw new CMakeExecutionException(
                $"CMake build failed with error code {errorCode}.");
        }
    }

    private static StartInfo CreateStartInfoForBuild(CMakeExecutionOptions options)
    {
        var startInfo = new StartInfo
        {
            FileName = "cmake",
            Arguments = new List<string>
            {
                "--build", options.BuildDirectory
            },
            OutputReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ProgressReceived?.Invoke(sender, new ProgressReceivedEventArgs(args.Data));
                }
            },
            ErrorReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ErrorReceived?.Invoke(sender, new ErrorReceivedEventArgs(args.Data));
                }
            }
        };

        if (!string.IsNullOrEmpty(options.AdditionalArgsForBuild))
        {
            startInfo.Arguments.Add(options.AdditionalArgsForBuild);
        }

        return startInfo;
    }

    private static StartInfo CreateStartInfoForConfigure(CMakeExecutionOptions options)
    {
        var startInfo = new StartInfo
        {
            FileName = "cmake",
            Arguments = new List<string>
            {
                "-S", options.CMakeListsFolder,
                "-B", options.BuildDirectory
            },
            OutputReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ProgressReceived?.Invoke(sender, new ProgressReceivedEventArgs(args.Data));
                }
            },
            ErrorReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ErrorReceived?.Invoke(sender, new ErrorReceivedEventArgs(args.Data));
                }
            }
        };

        if (!string.IsNullOrEmpty(options.AdditionalArgsForConfigure))
        {
            startInfo.Arguments.Add(options.AdditionalArgsForConfigure);
        }

        return startInfo;
    }
}