namespace CppPad.FileSystem;

public class Downloader(DiskFileSystem fileSystem)
{
    private readonly HttpClient _httpClient = new();

    public virtual async Task DownloadFileAsync(Uri uri, string destinationPath,
        CancellationToken token)
    {
        using var response =
            await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();

        await using var fs = await fileSystem.OpenWriteAsync(destinationPath);
        await response.Content.CopyToAsync(fs, token);
    }
}