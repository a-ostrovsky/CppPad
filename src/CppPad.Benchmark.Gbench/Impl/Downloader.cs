#region

using CppPad.Benchmark.Gbench.Interface;
using CppPad.FileSystem;

#endregion

namespace CppPad.Benchmark.Gbench.Impl;

public class HttpBenchmarkDownloader(DiskFileSystem fileSystem) : IBenchmarkDownloader
{
    private readonly HttpClient _httpClient = new();

    public async Task DownloadFileAsync(Uri uri, string destinationPath, CancellationToken token)
    {
        using var response =
            await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();

        await using var fs = await fileSystem.OpenWriteAsync(destinationPath);
        await response.Content.CopyToAsync(fs, token);
    }
}