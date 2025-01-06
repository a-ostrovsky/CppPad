namespace CppPad.SystemAdapter.IO;

public class SpecialFolders(string rootFolder)
{
    public string TempFolder { get; } = Path.Combine(rootFolder, "Temp");

    public string SettingsFolder { get; } = Path.Combine(rootFolder, "Settings");

    public string ToolsFolder { get; } = Path.Combine(rootFolder, "Tools");
}
