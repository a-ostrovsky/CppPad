#region

using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using CppPad.FileSystem;

#endregion

namespace CppPad.MockFileSystem;

public sealed class InMemoryFileSystem : DiskFileSystem
{
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, string> _files =
        new(StringComparer.OrdinalIgnoreCase);

    private bool _alwaysCreateDirectoriesIfNotExist;

    public InMemoryFileSystem()
    {
        CreateDirectory(@"C:\");
    }

    public void AlwaysCreateDirectoriesIfNotExist(bool alwaysCreate = true)
    {
        _alwaysCreateDirectoriesIfNotExist = alwaysCreate;
    }

    public override Task WriteAllTextAsync(string path, string contents)
    {
        WriteAllText(path, contents);
        return Task.CompletedTask;
    }

    public override void WriteAllText(string path, string contents)
    {
        EnsureDirectoryOfFileExists(path);
        _files[path] = contents;
    }

    public override Stream OpenWrite(string path)
    {
        var memoryStream = new MemoryStream();
        memoryStream.Position =
            memoryStream.Length; // Set the position to the end of the stream for writing
        _files[path] = string.Empty; // Initialize the file content

        var delegatingStream = new DelegatingStream(memoryStream);
        delegatingStream.Disposing += (_, _) =>
        {
            // Update the file content when the stream is disposed
            _files[path] = Encoding.UTF8.GetString(memoryStream.ToArray());
        };

        return delegatingStream;
    }

    public override Task<Stream> OpenWriteAsync(string path)
    {
        return Task.FromResult(OpenWrite(path));
    }

    public override void WriteAllLines(string path, string[] contents)
    {
        EnsureFileExists(path);
        _files[path] = string.Join(Environment.NewLine, contents);
    }

    public override Task WriteAllLinesAsync(string path, string[] contents)
    {
        WriteAllLines(path, contents);
        return Task.CompletedTask;
    }

    public override string ReadAllText(string path)
    {
        EnsureFileExists(path);
        return _files[path];
    }

    public override string[] ReadAllLines(string path)
    {
        EnsureFileExists(path);
        return _files[path].Split([Environment.NewLine], StringSplitOptions.None);
    }

    public override Task<string[]> ReadAllLinesAsync(string path)
    {
        return Task.FromResult(ReadAllLines(path));
    }

    public override Task<string> ReadAllTextAsync(string path)
    {
        return Task.FromResult(ReadAllText(path));
    }

    public override Stream OpenRead(string path)
    {
        EnsureFileExists(path);
        return new MemoryStream(Encoding.UTF8.GetBytes(_files[path]));
    }

    public override Task<Stream> OpenReadAsync(string path)
    {
        return Task.FromResult(OpenRead(path));
    }

    public override void CreateDirectory(string path)
    {
        var directoriesToCreate = new Stack<string>();
        var currentDirectory = path;

        while (!string.IsNullOrEmpty(currentDirectory) && !_directories.Contains(currentDirectory))
        {
            directoriesToCreate.Push(currentDirectory);
            currentDirectory = Path.GetDirectoryName(currentDirectory);
        }

        while (directoriesToCreate.Count > 0)
        {
            _directories.Add(directoriesToCreate.Pop());
        }
    }

    public override Task CreateDirectoryAsync(string path)
    {
        CreateDirectory(path);
        return Task.CompletedTask;
    }

    public override bool FileExists(string path)
    {
        return _files.ContainsKey(path);
    }

    public override bool DirectoryExists(string path)
    {
        return _directories.Contains(path);
    }

    public override string[] ListFiles(string path)
    {
        return _files.Keys.Where(f => Path.GetDirectoryName(f) == path).ToArray();
    }

    public override Task<string[]> ListFilesAsync(string path)
    {
        return Task.FromResult(ListFiles(path));
    }

    public override Task<string[]> ListFilesAsync(string path, string searchPattern)
    {
        // Convert the search pattern to a regex pattern
        var regexPattern = "^" + Regex.Escape(searchPattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        var files = _files.Keys
            .Where(f => Path.GetDirectoryName(f) == path && regex.IsMatch(Path.GetFileName(f)))
            .ToArray();

        return Task.FromResult(files);
    }

    public override Task<string[]> ListFilesAsync(string path, string searchPattern,
        SearchOption searchOption)
    {
        // Convert the search pattern to a regex pattern
        var regexPattern = "^" + Regex.Escape(searchPattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        var files = _files.Keys
            .Where(IsMatch)
            .ToArray();

        return Task.FromResult(files);

        bool IsMatch(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (searchOption == SearchOption.AllDirectories)
            {
                return directory != null &&
                       directory.StartsWith(path, StringComparison.OrdinalIgnoreCase) &&
                       regex.IsMatch(Path.GetFileName(filePath));
            }

            return directory != null &&
                   string.Equals(directory, path, StringComparison.OrdinalIgnoreCase) &&
                   regex.IsMatch(Path.GetFileName(filePath));
        }
    }

    public override void DeleteFile(string path)
    {
        _files.TryRemove(path, out _);
    }

    public override void DeleteDirectory(string path)
    {
        if (!_directories.Contains(path))
        {
            throw new DirectoryNotFoundException($"The directory '{path}' was not found.");
        }

        // Get all files in the directory and its subdirectories
        var filesToDelete = _files.Keys
            .Where(f => f.StartsWith(path, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var file in filesToDelete)
        {
            _files.TryRemove(file, out _);
        }

        // Get all subdirectories
        var directoriesToDelete = _directories
            .Where(d => d.StartsWith(path, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var directory in directoriesToDelete)
        {
            _directories.Remove(directory);
        }
    }

    public override Task CopyFileAsync(string sourceFileName, string destFileName)
    {
        EnsureFileExists(sourceFileName);
        EnsureDirectoryOfFileExists(destFileName);
        _files[destFileName] = _files[sourceFileName];
        return Task.CompletedTask;
    }

    public override async Task UnzipAsync(string zipFilePath, string extractPath,
        CancellationToken token = default)
    {
        EnsureFileExists(zipFilePath);
        EnsureDirectoryOfFileExists(extractPath);

        using var zipStream = new MemoryStream(Encoding.UTF8.GetBytes(_files[zipFilePath]));
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            token.ThrowIfCancellationRequested(); // Check for cancellation

            var entryPath = Path.Combine(extractPath, entry.FullName);
            if (string.IsNullOrEmpty(entry.Name))
            {
                // Entry is a directory
                await CreateDirectoryAsync(entryPath);
            }
            else
            {
                // Entry is a file
                await using var entryStream = entry.Open();
                using var memoryStream = new MemoryStream();
                await entryStream.CopyToAsync(memoryStream, token);
                _files[entryPath] = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }


    private void EnsureFileExists(string filePath)
    {
        EnsureDirectoryOfFileExists(filePath);
        if (!_files.TryGetValue(filePath, out _))
        {
            throw new FileNotFoundException($"The file '{filePath}' was not found.");
        }
    }

    private void EnsureDirectoryOfFileExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath)!;
        if (_alwaysCreateDirectoriesIfNotExist)
        {
            CreateDirectory(directory);
        }
        else if (!_directories.Contains(directory))
        {
            throw new DirectoryNotFoundException($"The directory '{directory}' was not found.");
        }
    }
}