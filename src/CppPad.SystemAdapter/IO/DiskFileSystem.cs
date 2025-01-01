#region

using System.IO.Compression;

#endregion

namespace CppPad.SystemAdapter.IO;

public class DiskFileSystem
{
    public SpecialFolders SpecialFolders { get; } = 
        new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
    
    public virtual Task<string> ReadAllTextAsync(string path)
    {
        return File.ReadAllTextAsync(path);
    }

    public virtual Task WriteAllTextAsync(string path, string contents)
    {
        return File.WriteAllTextAsync(path, contents);
    }

    public virtual Task<Stream> OpenReadAsync(string path)
    {
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public virtual Task<Stream> OpenWriteAsync(string path)
    {
        return Task.FromResult<Stream>(File.OpenWrite(path));
    }

    public virtual Task<string[]> ListFilesAsync(string path)
    {
        return Task.Run(() => Directory.GetFiles(path));
    }

    public virtual Task<string[]> ListFilesAsync(string path, string searchPattern)
    {
        return Task.Run(() => Directory.GetFiles(path, searchPattern));
    }

    public virtual Task<string[]> ListFilesAsync(string path, string searchPattern,
        SearchOption searchOption)
    {
        return Task.Run(() => Directory.GetFiles(path, searchPattern, searchOption));
    }

    public virtual Task CreateDirectoryAsync(string path)
    {
        return Task.Run(() => Directory.CreateDirectory(path));
    }

    public virtual void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public virtual string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public virtual string[] ReadAllLines(string path)
    {
        return File.ReadAllLines(path);
    }

    public virtual Task<string[]> ReadAllLinesAsync(string path)
    {
        return File.ReadAllLinesAsync(path);
    }

    public virtual void WriteAllLines(string path, string[] contents)
    {
        File.WriteAllLines(path, contents);
    }

    public virtual Task WriteAllLinesAsync(string path, string[] contents)
    {
        return File.WriteAllLinesAsync(path, contents);
    }

    public virtual void WriteAllText(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    public virtual Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }

    public virtual Stream OpenWrite(string path)
    {
        return File.OpenWrite(path);
    }

    public virtual string[] ListFiles(string path)
    {
        return Directory.GetFiles(path);
    }

    public virtual bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public virtual bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public virtual void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public virtual void DeleteDirectory(string path)
    {
        // Use the \\?\ prefix to handle long paths
        var longPath = @"\\?\" + Path.GetFullPath(path);

        if (!Directory.Exists(longPath))
        {
            return;
        }

        RemoveReadOnlyAttributes(longPath);

        Directory.Delete(longPath, true);
    }

    public virtual Task CopyFileAsync(string sourceFileName, string destFileName)
    {
        return Task.Run(() => File.Copy(sourceFileName, destFileName, true));
    }

    public virtual async Task UnzipAsync(string zipFilePath, string extractPath,
        CancellationToken cancellationToken = default)
    {
        // Ensure the cancellation token is checked at the start
        cancellationToken.ThrowIfCancellationRequested();

        // Open the zip file
        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            // Check for cancellation before processing each entry
            cancellationToken.ThrowIfCancellationRequested();

            var destinationPath = Path.Combine(extractPath, entry.FullName);

            // Create the directory if it doesn't exist
            if (entry.FullName.EndsWith('/'))
            {
                Directory.CreateDirectory(destinationPath);
            }
            else
            {
                // Ensure the directory for the file exists
                var destinationFolder = Path.GetDirectoryName(destinationPath);
                if (destinationFolder != null)
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Extract the file
                await using var entryStream = entry.Open();
                await using var fileStream =
                    new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                await entryStream.CopyToAsync(fileStream, cancellationToken);
            }
        }
    }

    private static void RemoveReadOnlyAttributes(string path)
    {
        var directoryInfo = new DirectoryInfo(path);

        foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }
        }

        foreach (var subDirectoryInfo in directoryInfo.GetDirectories("*",
                     SearchOption.AllDirectories))
        {
            if ((subDirectoryInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                subDirectoryInfo.Attributes &= ~FileAttributes.ReadOnly;
            }
        }
    }
}