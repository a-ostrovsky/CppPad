#region

using CppPad.Common;
using CppPad.FileSystem;

#endregion

namespace CppPad.Configuration.Json;

public class SettingsFile(DiskFileSystem fileSystem)
{
    private const string SettingsFileName = "config.json";

    public string GetOrCreateFile()
    {
        fileSystem.CreateDirectory(AppConstants.AppFolder);
        return Path.Combine(AppConstants.AppFolder, SettingsFileName);
    }
}