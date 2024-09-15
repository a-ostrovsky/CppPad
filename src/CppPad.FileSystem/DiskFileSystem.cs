using CppPad.Common;

namespace CppPad.FileSystem;

public class DiskFileSystem
{
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

    public virtual string CreateTempFile(string? extensions)
    {
        var tempFolder = AppConstants.TempFolder;
        Directory.CreateDirectory(AppConstants.TempFolder);
        var fileName = Path.Combine(tempFolder,
            extensions == null
                ? $"{Guid.NewGuid()}"
                : $"{Guid.NewGuid()}.{extensions}");
        File.WriteAllText(fileName, string.Empty);
        return fileName;
    }

    public virtual void DeleteFile(string path)
    {
        File.Delete(path);
    }
}
