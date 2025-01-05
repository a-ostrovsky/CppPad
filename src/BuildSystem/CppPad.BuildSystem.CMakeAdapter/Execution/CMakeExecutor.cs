using CppPad.EnvironmentConfiguration;
using CppPad.SystemAdapter.Execution;
using CppPad.SystemAdapter.IO;

namespace CppPad.BuildSystem.CMakeAdapter.Execution;

public class CMakeExecutor(DiskFileSystem fileSystem, Process process)
{
    public async Task RunCMakeAsync(
        CMakeExecutionOptions options,
        CancellationToken cancellationToken = default
    )
    {
        if (!fileSystem.DirectoryExists(options.BuildDirectory))
        {
            await fileSystem.CreateDirectoryAsync(options.BuildDirectory);
        }

        var cmakeExecutablePath = GetCMakeExecutablePath(options.EnvironmentSettings);

        await ConfigureAsync(cmakeExecutablePath, options, cancellationToken);
        await BuildAsync(cmakeExecutablePath, options, cancellationToken);
    }

    private static string GetCMakeBuildType(BuildMode buildMode)
    {
        return buildMode switch
        {
            BuildMode.Debug => "Debug",
            BuildMode.Release => "Release",
            _ => throw new ArgumentException($"Unsupported configuration: {buildMode}"),
        };
    }

    private async Task ConfigureAsync(
        string cmakeExecutablePath,
        CMakeExecutionOptions options,
        CancellationToken token = default
    )
    {
        token.ThrowIfCancellationRequested();
        if (!options.ForceConfigure)
        {
            var configureMarkerFile = Path.Combine(options.BuildDirectory, "CMakeCache.txt");
            if (fileSystem.FileExists(configureMarkerFile))
            {
                return;
            }
        }

        var startInfo = CreateStartInfoForConfigure(cmakeExecutablePath, options);
        var processInfo = process.Start(startInfo);
        var errorCode = await process.WaitForExitAsync(processInfo, token);
        if (errorCode != 0)
        {
            throw new CMakeExecutionException(
                $"CMake configure failed with error code {errorCode}."
            );
        }
    }

    private async Task BuildAsync(
        string cmakeExecutablePath,
        CMakeExecutionOptions options,
        CancellationToken token = default
    )
    {
        token.ThrowIfCancellationRequested();
        var startInfo = CreateStartInfoForBuild(cmakeExecutablePath, options);
        var processInfo = process.Start(startInfo);
        var errorCode = await process.WaitForExitAsync(processInfo, token);
        if (errorCode != 0)
        {
            throw new CMakeExecutionException($"CMake build failed with error code {errorCode}.");
        }
    }

    private string GetCMakeExecutablePath(EnvironmentSettings environmentSettings)
    {
        var paths =
            environmentSettings
                .TryGet("PATH")
                ?.Split(Path.PathSeparator, StringSplitOptions.TrimEntries)
            ?? throw new CMakeExecutionException("Environment variables do not contain PATH");
        var cmakePath =
            paths.Select(p => Path.Combine(p, "cmake")).FirstOrDefault(fileSystem.FileExists)
            ?? paths
                .Select(p => Path.Combine(p, "cmake.exe"))
                .FirstOrDefault(fileSystem.FileExists);
        if (cmakePath is null)
        {
            throw new CMakeExecutionException("CMake not found in PATH");
        }

        return cmakePath;
    }

    private static StartInfo CreateStartInfoForBuild(
        string cmakeExecutablePath,
        CMakeExecutionOptions options
    )
    {
        var startInfo = new StartInfo
        {
            FileName = cmakeExecutablePath,
            Arguments =
            [
                "--build",
                options.BuildDirectory,
                "--config",
                GetCMakeBuildType(options.BuildMode),
            ],
            OutputReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ProgressReceived?.Invoke(
                        sender,
                        new ProgressReceivedEventArgs(args.Data)
                    );
                }
            },
            ErrorReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ErrorReceived?.Invoke(sender, new ErrorReceivedEventArgs(args.Data));
                }
            },
        };

        if (!string.IsNullOrEmpty(options.AdditionalArgsForBuild))
        {
            startInfo.Arguments.Add(options.AdditionalArgsForBuild);
        }

        return startInfo;
    }

    private static StartInfo CreateStartInfoForConfigure(
        string cmakeExecutablePath,
        CMakeExecutionOptions options
    )
    {
        var startInfo = new StartInfo
        {
            FileName = cmakeExecutablePath,
            Arguments =
            [
                "-S",
                options.CMakeListsFolder,
                "-B",
                options.BuildDirectory,
                $"-DCMAKE_BUILD_TYPE={GetCMakeBuildType(options.BuildMode)}",
            ],
            OutputReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ProgressReceived?.Invoke(
                        sender,
                        new ProgressReceivedEventArgs(args.Data)
                    );
                }
            },
            ErrorReceived = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    options.ErrorReceived?.Invoke(sender, new ErrorReceivedEventArgs(args.Data));
                }
            },
        };

        if (!string.IsNullOrEmpty(options.AdditionalArgsForConfigure))
        {
            startInfo.Arguments.Add(options.AdditionalArgsForConfigure);
        }

        return startInfo;
    }
}
