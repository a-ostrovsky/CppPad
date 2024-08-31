namespace CppPad.Common;

public static class AppConstants
{
    public const string AppName = "CppPad";

    public const string FileFilter =
        "C++ Files (*.cpp)|*.cpp|C Files (*.c)|*.c|All Files (*.*)|*.*";

    public static readonly string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

    public static readonly string TempFolder = Path.Combine(AppFolder, "Temp");
}