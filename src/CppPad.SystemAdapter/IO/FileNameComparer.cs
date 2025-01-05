namespace CppPad.SystemAdapter.IO;

public class FileNameComparer : IComparer<string>
{
    public virtual int Compare(string? x, string? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        return string.Compare(x, y,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}