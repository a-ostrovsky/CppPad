#region

using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

#endregion

namespace CppPad.LanguageServer.ClangD;

public class GitHubDownloader(
    Uri releasesUri,
    string targetFolder,
    ILoggerFactory loggerFactory)
{
    private const string DefaultUri =
        "https://api.github.com/repos/clangd/clangd/releases/latest";

    private readonly HttpClient _client = new();

    private readonly ILogger _logger =
        loggerFactory.CreateLogger<GitHubDownloader>();

    public GitHubDownloader(string targetFolder,
        ILoggerFactory loggerFactory)
        : this(new Uri(DefaultUri), targetFolder, loggerFactory)
    {
    }

    public event EventHandler<ProgressEventArgs>? ProgressChanged;

    public async Task DownloadAsync(
        CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }

        Directory.CreateDirectory(targetFolder);
        var zipFilePath = Path.Combine(targetFolder, "clangd.zip");
        File.Delete(zipFilePath);

        _client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (compatible;)");
        var releaseInfoSteam =
            await _client.GetStreamAsync(releasesUri, cancellationToken);
        var releaseJson = await JsonDocument.ParseAsync(releaseInfoSteam,
            new JsonDocumentOptions(), cancellationToken);
        var assets = releaseJson.RootElement.GetProperty("assets")
            .EnumerateArray().ToArray();
        _logger.LogInformation("Found {Count} assets in the release.",
            assets.Length);
        var assetsForWindows =
            assets.Where(IsClangdAssetForWindows).ToArray();
        _logger.LogInformation(
            "Found {Count} assets for Windows in the release.",
            assetsForWindows.Length);
        if (assetsForWindows.Length == 0)
        {
            return;
        }

        OnProgressChanged("Found version to download.");

        var asset = assetsForWindows[0];
        var downloadUrl =
            asset.GetProperty("browser_download_url").GetString();
        OnProgressChanged($"Starting download from <{downloadUrl}>.");
        using (var response = await _client.GetAsync(downloadUrl, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            await using var fs = new FileStream(zipFilePath,
                FileMode.Create,
                FileAccess.Write, FileShare.None);
            OnProgressChanged("Downloading...");
            await response.Content.CopyToAsync(fs, cancellationToken);
            OnProgressChanged("Download finished");
        }

        OnProgressChanged("Unpacking...");
        ZipFile.ExtractToDirectory(zipFilePath, targetFolder, true);
        OnProgressChanged("Unpacking finished");
        OnProgressChanged("Delete temporary files");
        File.Delete(zipFilePath);
        OnProgressChanged("Done");
    }

    private static bool IsClangdAssetForWindows(JsonElement asset)
    {
        var nameProperty = asset.GetProperty("name").GetString();
        if (nameProperty == null)
        {
            return false;
        }

        return nameProperty.Contains("clangd-windows-",
            StringComparison.Ordinal) && nameProperty.EndsWith(".zip");
    }

    private void OnProgressChanged(string message)
    {
        ProgressChanged?.Invoke(this, new ProgressEventArgs(message));
    }
}