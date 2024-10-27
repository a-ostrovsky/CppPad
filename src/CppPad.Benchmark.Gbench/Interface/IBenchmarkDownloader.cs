namespace CppPad.Benchmark.Gbench.Interface;

public interface IBenchmarkDownloader
{
    Task DownloadFileAsync(Uri uri, string destinationPath, CancellationToken token);
}