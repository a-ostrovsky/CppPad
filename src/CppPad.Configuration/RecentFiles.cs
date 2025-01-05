using CppPad.SystemAdapter.IO;

namespace CppPad.Configuration;

public class RecentFiles(DiskFileSystem fileSystem)
{
    public const int MaxRecentFiles = 10;

    private readonly string _recentFilesPath = Path.Combine(
        fileSystem.SpecialFolders.SettingsFolder,
        "RecentFiles.txt"
    );

    public async Task AddAsync(string path)
    {
        var recentFiles = await LoadRecentFilesAsync();
        recentFiles.RemoveAll(f => fileSystem.FileNameComparer.Compare(f, path) == 0);
        recentFiles.Insert(0, path);
        if (recentFiles.Count > MaxRecentFiles)
        {
            recentFiles.RemoveRange(MaxRecentFiles, recentFiles.Count - MaxRecentFiles);
        }

        await SaveRecentFilesAsync(recentFiles);
        RecentFilesChanged?.Invoke(this, new RecentFilesChangedEventArgs(recentFiles));
    }

    public async Task RemoveAsync(string path)
    {
        var recentFiles = await LoadRecentFilesAsync();
        recentFiles.RemoveAll(f => fileSystem.FileNameComparer.Compare(f, path) == 0);
        await SaveRecentFilesAsync(recentFiles);
        RecentFilesChanged?.Invoke(this, new RecentFilesChangedEventArgs(recentFiles));
    }

    private async Task SaveRecentFilesAsync(IEnumerable<string> recentFiles)
    {
        await fileSystem.CreateDirectoryAsync(Path.GetDirectoryName(_recentFilesPath)!);
        await fileSystem.WriteAllLinesAsync(_recentFilesPath, [.. recentFiles]);
    }

    public async Task<List<string>> LoadRecentFilesAsync()
    {
        var result = fileSystem.FileExists(_recentFilesPath)
            ? (await fileSystem.ReadAllLinesAsync(_recentFilesPath)).ToList()
            : [];
        return result;
    }

    public event EventHandler<RecentFilesChangedEventArgs>? RecentFilesChanged;
}
