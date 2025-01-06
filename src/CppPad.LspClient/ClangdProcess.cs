using System.Text;
using CppPad.Common;
using CppPad.Logging;
using CppPad.SystemAdapter.Execution;
using CppPad.SystemAdapter.IO;
using Microsoft.Extensions.Logging;

namespace CppPad.LspClient;

public class ClangdProcess : ILspProcess, IDisposable
{
    private const string ClangdZipFileName = "clangd-windows.zip";
    private readonly CancellationTokenSource _cts = new();
    private readonly DiskFileSystem _fileSystem;
    private readonly AsyncLock _lock = new();
    private readonly ILogger _logger = Log.CreateLogger<ClangdProcess>();
    private readonly Process _process;
    private IProcessInfo? _processInfo;

    public ClangdProcess(Process process, DiskFileSystem fileSystem)
    {
        _process = process;
        _fileSystem = fileSystem;
        Task.Run(EnsureClangdExistsAsync);
    }

    private string ClangdStatusFileName =>
        Path.Combine(_fileSystem.SpecialFolders.ToolsFolder, "clangd_status.txt");

    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task StartAsync()
    {
        var exeFileName = await EnsureClangdExistsAsync();
        using var lck = await _lock.LockAsync();
        var processInfo = _process.Start(
            new StartInfo { FileName = exeFileName, RedirectIoStreams = true }
        );
        _processInfo = processInfo;
        InputWriter = new StreamWriter(
            _processInfo.GetStandardInput().BaseStream,
            new UTF8Encoding(false)
        )
        {
            AutoFlush = true,
        };
        OutputReader = new StreamReader(
            _processInfo.GetStandardOutput().BaseStream,
            new UTF8Encoding(false)
        );
    }

    public async Task KillAsync()
    {
        using var lck = await _lock.LockAsync();
        if (_processInfo != null)
        {
            _process.Kill(_processInfo);
        }
    }

    public TextReader? OutputReader { get; private set; }
    public TextWriter? InputWriter { get; private set; }
    public bool HasExited => _processInfo?.HasExited ?? true;

    private async Task<string> EnsureClangdExistsAsync()
    {
        using var lck = await _lock.LockAsync();
        if (_fileSystem.FileExists(ClangdStatusFileName))
        {
            _logger.LogInformation(
                "clangd status file exists: {ClangdStatusFileName}.",
                ClangdStatusFileName
            );
            var clangdExeFile = await _fileSystem.ReadAllTextAsync(ClangdStatusFileName);
            _logger.LogInformation("clangd exe file: {clangdExeFile}.", clangdExeFile);
            if (_fileSystem.FileExists(clangdExeFile))
            {
                return clangdExeFile;
            }

            _logger.LogInformation("clangd exe file not found: {clangdExeFile}.", clangdExeFile);
        }

        if (_fileSystem.DirectoryExists(_fileSystem.SpecialFolders.ToolsFolder))
        {
            await _fileSystem.CreateDirectoryAsync(_fileSystem.SpecialFolders.ToolsFolder);
        }

        var clangdZipFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            ClangdZipFileName
        );
        var targetFolder = _fileSystem.SpecialFolders.ToolsFolder;
        await _fileSystem.UnzipAsync(clangdZipFilePath, targetFolder, _cts.Token);
        var clangdPath = (
            await _fileSystem.ListFilesAsync(
                targetFolder,
                "clangd.exe",
                SearchOption.AllDirectories
            )
        ).FirstOrDefault();
        if (clangdPath == null)
        {
            _logger.LogError("clangd.exe not found in the zip file.");
            throw new InvalidOperationException("clangd.exe not found in the zip file.");
        }

        await _fileSystem.WriteAllTextAsync(
            Path.Combine(targetFolder, "clangd_status.txt"),
            clangdPath
        );
        _logger.LogInformation("Clangd extracted to {clangdPath}.", clangdPath);
        return clangdPath;
    }
}
