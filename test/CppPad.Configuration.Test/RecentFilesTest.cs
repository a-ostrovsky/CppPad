using CppPad.MockSystemAdapter;

namespace CppPad.Configuration.Test;

public class RecentFilesTest
{
    private readonly RecentFiles _recentFiles;

    public RecentFilesTest()
    {
        var fileSystem = new InMemoryFileSystem();
        _recentFiles = new RecentFiles(fileSystem);
    }

    [Fact]
    public async Task Add_single_recent_file()
    {
        var receivedUpdates = new List<string>();
        _recentFiles.RecentFilesChanged += (_, args) => { receivedUpdates.AddRange(args.RecentFiles); };
        await _recentFiles.AddAsync("test.cpp");
        Assert.Contains("test.cpp", await _recentFiles.LoadRecentFilesAsync());
        Assert.Contains("test.cpp", receivedUpdates);
    }

    [Fact]
    public async Task Add_two_recent_files()
    {
        await _recentFiles.AddAsync("test1.cpp");
        await _recentFiles.AddAsync("test2.cpp");
        Assert.Equal(["test2.cpp", "test1.cpp"], await _recentFiles.LoadRecentFilesAsync());
    }

    [Fact]
    public async Task Add_same_recent_file_twice_files()
    {
        await _recentFiles.AddAsync("test1.cpp");
        await _recentFiles.AddAsync("test1.cpp");
        Assert.Equal(["test1.cpp"], await _recentFiles.LoadRecentFilesAsync());
    }

    [Fact]
    public async Task Add_too_many_recent_files()
    {
        for (var i = 0; i < RecentFiles.MaxRecentFiles; i++)
        {
            await _recentFiles.AddAsync($"test{i}.cpp");
        }

        await _recentFiles.AddAsync("testNNN.cpp");
        Assert.DoesNotContain("test0.cpp", await _recentFiles.LoadRecentFilesAsync());
    }
}