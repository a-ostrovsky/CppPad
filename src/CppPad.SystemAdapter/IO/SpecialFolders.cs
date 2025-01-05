namespace CppPad.SystemAdapter.IO;

public class SpecialFolders(string rootFolder)
{
    public virtual string TempFolder { get; } = Path.Combine(rootFolder, "Temp");
}
