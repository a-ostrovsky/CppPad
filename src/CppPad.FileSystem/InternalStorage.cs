namespace CppPad.FileSystem;

public class InternalStorage(DiskFileSystem fileSystem, string rootFolder)
{
    public virtual void Init()
    {
        fileSystem.CreateDirectory(rootFolder);
    }

    public virtual Task InitAsync()
    {
        return fileSystem.CreateDirectoryAsync(rootFolder);
    }

    public virtual void Format()
    {
        fileSystem.DeleteDirectory(rootFolder);
        Init();
    }

    public virtual Task FormatAsync()
    {
        fileSystem.DeleteDirectory(rootFolder);
        return fileSystem.CreateDirectoryAsync(rootFolder);
    }
}