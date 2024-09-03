#region

using CppPad.Common;
using CppPad.FileSystem;
using System.Collections.Concurrent;
using System.Text;

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
        CreateDirectory(AppConstants.AppFolder);
        CreateDirectory(AppConstants.TempFolder);
    }

    public void AlwaysCreateDirectoriesIfNotExist()
    {
        _alwaysCreateDirectoriesIfNotExist = true;
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

    public override string[] ListFiles(string path)
    {
        return _files.Keys.Where(f => Path.GetDirectoryName(f) == path).ToArray();
    }

    public override Task<string[]> ListFilesAsync(string path)
    {
        return Task.FromResult(ListFiles(path));
    }

    public override string CreateTempFile(string? extensions)
    {
        var tempFolder = AppConstants.TempFolder;
        CreateDirectory(tempFolder);

        var guid = Guid.NewGuid().ToString();

        var fileName = Path.Combine(tempFolder,
            extensions == null
                ? $"{guid}"
                : $"{guid}.{extensions}");
        _files[fileName] = string.Empty;
        return fileName;
    }

    public override void DeleteFile(string path)
    {
        _files.TryRemove(path, out _);
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