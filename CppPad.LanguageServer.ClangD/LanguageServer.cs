#region

using CppPad.Common;
using CppPad.LanguageServer.Interface;
using CppPad.LanguageServerAdapter;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

#endregion

namespace CppPad.LanguageServer.ClangD;

public class LanguageServer(ILoggerFactory loggerFactory) : ILanguageServer, IAsyncDisposable
{
    private const string InstallMessage =
        "ClangD is not installed. Do you want to download it?";

    private static readonly string ClangdFolder =
        Path.Combine(AppConstants.AppFolder, "clangd");

    private readonly GitHubDownloader _downloader = new(ClangdFolder, loggerFactory);

    private readonly ILogger _logger = loggerFactory.CreateLogger<LanguageServer>();

    private readonly ClangdResolver _resolver = new(ClangdFolder, loggerFactory);

    private Adapter? _adapter;
    private Process? _clangdProcess;

    public async Task<IList<AutoCompletionData>> GetAutoCompletionAsync(
        FileData fileData, Position position, CancellationToken token = default)
    {
        if (_adapter == null)
        {
            StartAdapter();
            Debug.Assert(_adapter != null);
            await _adapter.StartAsync(token);
        }

        string fileUri = fileData.FileUri;
        if (fileData.FileUri.StartsWith("untitled:///"))
        {
            var tempFile = Path.Combine(AppConstants.TempFolder, "tmp.cpp");
            await File.WriteAllTextAsync(tempFile, fileData.Content, token);
            fileUri = new Uri(tempFile, UriKind.Absolute).AbsoluteUri;
        }

        await _adapter.OpenOrUpdateFile(fileUri, fileData.Content, token);
        var completionData = await _adapter.RequestCompletionAsync(fileUri, position.Line,
            position.Character, token);
        var result = completionData
            .OrderBy(item => item.SortText)
            .Select(item => new AutoCompletionData(item.Label, item.InsertText))
            .ToArray();
        return result;
    }

    public async Task InstallAsync(IInstallCallbacks callbacks,
        CancellationToken token)
    {
        if (_resolver.TryGetClangdExecutable() != null)
        {
            _logger.LogInformation("Clangd is already installed.");
            return;
        }

        if (!await callbacks.ConfirmInstallationAsync(InstallMessage))
        {
            _logger.LogInformation("Installation cancelled.");
            return;
        }

        _downloader.ProgressChanged += (_, e) => { callbacks.OnProgress(e.Message); };
        await _downloader.DownloadAsync(token);
        _logger.LogInformation("Clangd download succeeded.");
    }

    private void StartAdapter()
    {
        _logger.LogInformation("Starting adapter.");
        var clangdPath = _resolver.TryGetClangdExecutable();
        if (clangdPath == null)
        {
            throw new InvalidOperationException("Clangd is not installed.");
        }

        _logger.LogInformation("Clangd path: {clangdPath}", clangdPath);
        StartClangd(clangdPath);
        Debug.Assert(_clangdProcess != null);
        var communicator = new LanguageServerCommunicator(
            _clangdProcess.StandardInput, _clangdProcess.StandardOutput,
            loggerFactory);
        var workflow = new LanguageServerWorkflow(communicator);
        _adapter = new Adapter(workflow, loggerFactory);
        _logger.LogInformation("Adapter started.");
    }

    private void StartClangd(string clangdPath)
    {
        if (_clangdProcess != null)
        {
            throw new InvalidOperationException(
                "The adapter has already been started.");
        }

        _clangdProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = clangdPath,
                Arguments = "--background-index",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _clangdProcess.Start();
    }

    public async ValueTask DisposeAsync()
    {
        if (_adapter != null)
        {
            await _adapter.StopAsync();
        }

        _clangdProcess?.Kill();
        _clangdProcess?.Dispose();
        _clangdProcess = null;

        GC.SuppressFinalize(this);
    }
}