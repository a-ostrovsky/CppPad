#region

using CppPad.Benchmark.Gbench.Interface;
using CppPad.Benchmark.Interface;
using CppPad.Common;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.Benchmark.Gbench.Impl;

public class BenchmarkInstaller(
    ILoggerFactory loggerFactory,
    DiskFileSystem fileSystem,
    IBenchmarkDownloader benchmarkDownloader,
    IBenchmarkBuilder benchmarkBuilder)
{
    private const string GoogleBenchmarkUrl =
        "https://github.com/google/benchmark/archive/refs/heads/main.zip";

    private const string CMakeUrl =
        "https://github.com/Kitware/CMake/releases/download/v3.27.4/cmake-3.27.4-windows-x86_64.zip";

    private readonly ILogger _logger = loggerFactory.CreateLogger<BenchmarkInstaller>();
    private readonly AsyncLock _newMessageLock = new();

    public bool IsBenchmarkInstalled()
    {
        var includeDir = Path.Combine(AppConstants.BenchmarkFolder, "include");
        var libDir = Path.Combine(AppConstants.BenchmarkFolder, "lib");

        return fileSystem.DirectoryExists(includeDir) && fileSystem.DirectoryExists(libDir) &&
               fileSystem.ListFiles(includeDir).Length != 0 &&
               fileSystem.ListFiles(libDir).Length != 0;
    }

    public async Task InstallAsync(IInitCallbacks callbacks, CancellationToken token = default)
    {
        var workDir = Path.Combine(AppConstants.TempFolder, "Benchmark");
        var googleBenchDir = Path.Combine(workDir, "benchmark");
        var cmakePath = Path.Combine(workDir, "cmake");

        callbacks.OnNewMessage($"Preparing work directory: '{workDir}'...");
        ClearWorkDir(workDir);

        var downloadTasks = new[]
        {
            DownloadAndExtractAsync(callbacks, GoogleBenchmarkUrl, googleBenchDir, "benchmark.zip",
                token),
            DownloadAndExtractAsync(callbacks, CMakeUrl, cmakePath, "cmake.zip", token)
        };

        await Task.WhenAll(downloadTasks);

        var benchmarkSourceDir = Path.Combine(googleBenchDir, "benchmark-main");
        var buildDir = Path.Combine(benchmarkSourceDir, "build");
        await benchmarkBuilder.BuildAsync(callbacks, cmakePath, benchmarkSourceDir,
            buildDir, token);

        await CopyBenchmarkFilesAsync(callbacks, benchmarkSourceDir, token);
    }

    private void ClearWorkDir(string path)
    {
        if (fileSystem.DirectoryExists(path))
        {
            _logger.LogInformation("Clearing work directory...");
            fileSystem.DeleteDirectory(path);
        }

        fileSystem.CreateDirectory(path);
    }

    private async Task DownloadAndExtractAsync(IInitCallbacks callbacks, string url,
        string extractDir, string zipFileName, CancellationToken token = default)
    {
        _logger.LogInformation(
            "Downloading from {url}. Extract dir: {extractDir}. Zip file name: {zipFileName}...",
            url, extractDir, zipFileName);

        using (await _newMessageLock.LockAsync())
        {
            callbacks.OnNewMessage($"Downloading from '{url}' to {zipFileName}...");
        }

        var zipPath = Path.Combine(extractDir, zipFileName);
        await fileSystem.CreateDirectoryAsync(extractDir);

        await benchmarkDownloader.DownloadFileAsync(new Uri(url), zipPath, token);

        using (await _newMessageLock.LockAsync())
        {
            callbacks.OnNewMessage($"Extracting {zipFileName} to '{extractDir}'...");
        }

        await fileSystem.UnzipAsync(zipPath, extractDir, token);

        using (await _newMessageLock.LockAsync())
        {
            callbacks.OnNewMessage($"Download finished: '{extractDir}'.");
        }
    }

    private async Task CopyBenchmarkFilesAsync(IInitCallbacks callbacks, string sourceDir,
        CancellationToken token)
    {
        var filesToCopy = new[]
        {
            "benchmark.lib",
            "benchmark_main.lib",
            "benchmark.h",
            "export.h"
        };

        var destIncludeDir = Path.Combine(AppConstants.BenchmarkFolder, "include");
        var destLibDir = Path.Combine(AppConstants.BenchmarkFolder, "lib");

        await fileSystem.CreateDirectoryAsync(destIncludeDir);
        await fileSystem.CreateDirectoryAsync(destLibDir);

        foreach (var fileName in filesToCopy)
        {
            var filePath = await FindFileAsync(sourceDir, fileName);
            if (filePath != null)
            {
                token.ThrowIfCancellationRequested();
                callbacks.OnNewMessage($"Found {fileName} at {filePath}");
                _logger.LogInformation("Found {fileName} at {filePath}", fileName, filePath);

                var destDir = fileName.EndsWith(".lib") ? destLibDir : destIncludeDir;
                var destFile = Path.Combine(destDir, fileName);
                await fileSystem.CopyFileAsync(filePath, destFile);
            }
            else
            {
                _logger.LogWarning("File {fileName} not found in {sourceDir}", fileName, sourceDir);
                throw new FileNotFoundException($"File {fileName} not found in {sourceDir}");
            }
        }
    }

    private async Task<string?> FindFileAsync(string rootDir, string fileName)
    {
        // List files in the current directory
        var files = await fileSystem.ListFilesAsync(rootDir, "*", SearchOption.AllDirectories);
        return files
            .AsParallel()
            .FirstOrDefault(file => Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }
}