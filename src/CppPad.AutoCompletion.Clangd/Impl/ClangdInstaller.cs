#region

using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.AutoCompletion.Clangd.Impl;

public class ClangdInstaller(
    ILoggerFactory loggerFactory,
    DiskFileSystem fileSystem,
    Downloader clangdDownloader)
{
    private const string ClangdUrl =
        "https://github.com/clangd/clangd/releases/download/19.1.2/clangd-windows-19.1.2.zip";

    private readonly ILogger _logger = loggerFactory.CreateLogger<ClangdInstaller>();
    private readonly AsyncLock _newMessageLock = new();

    public async Task InstallAsync(IInitCallbacks callbacks, CancellationToken token = default)
    {
        var workDir = Path.Combine(AppConstants.TempFolder, "Clangd");
        var clangdDir = Path.Combine(workDir, "clangd");

        callbacks.OnNewMessage($"Preparing work directory: '{workDir}'...");
        ClearWorkDir(workDir);

        await DownloadAndExtractAsync(callbacks, ClangdUrl, clangdDir, "clangd.zip", token);

        await CopyClangdFilesAsync(callbacks, clangdDir, token);
    }
    
    public bool IsInstalled()
    {
        var clangdPath = Path.Combine(AppConstants.ClangdFolder, "bin", "clangd.exe");
        return fileSystem.FileExists(clangdPath);
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

        await clangdDownloader.DownloadFileAsync(new Uri(url), zipPath, token);

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

    private async Task CopyClangdFilesAsync(IInitCallbacks callbacks, string sourceDir,
        CancellationToken token)
    {
        var filesToCopy = new[]
        {
            "clangd.exe"
        };

        var destDir = Path.Combine(AppConstants.ClangdFolder, "bin");

        await fileSystem.CreateDirectoryAsync(destDir);

        foreach (var fileName in filesToCopy)
        {
            var filePath = await FindFileAsync(sourceDir, fileName);
            if (filePath != null)
            {
                token.ThrowIfCancellationRequested();
                callbacks.OnNewMessage($"Found {fileName} at {filePath}");
                _logger.LogInformation("Found {fileName} at {filePath}", fileName, filePath);

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
        var files = await fileSystem.ListFilesAsync(rootDir, "*", SearchOption.AllDirectories);
        return files
            .AsParallel()
            .FirstOrDefault(file =>
                Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }
}