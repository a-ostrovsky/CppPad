using CppPad.EnvironmentConfiguration;
using CppPad.MockSystemAdapter;

namespace CppPad.BuildAndRun.Test.CMakeTests;

public static class CMakeInstaller
{
    public static EnvironmentSettings Install(InMemoryFileSystem fileSystem)
    {
        const string cmakePath = @"C:\cmake\bin\cmake.exe";
        var cmakeDir = Path.GetDirectoryName(cmakePath)!;
        fileSystem.CreateDirectory(cmakeDir);
        fileSystem.WriteAllText(cmakePath, string.Empty);
        var result = new EnvironmentSettings();
        result.Add("PATH", cmakeDir);
        return result;
    }
}
