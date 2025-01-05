namespace CppPad.Configuration;

public class RecentFilesChangedEventArgs(IReadOnlyList<string> recentFiles) : EventArgs
{
    public IReadOnlyList<string> RecentFiles => recentFiles;
}