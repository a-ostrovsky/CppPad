namespace CppPad.Common;

public static class AppConstants
{
    public const string AppName = "CppPad";

    public const string DefaultFileExtension = ".cpad";

    public const string OpenFileFilter =
        "CppPad Files (*.cpad)|*.cpad|C++ Files (*.cpp)|*.cpp|C Files (*.c)|*.c|All Files (*.*)|*.*";

    public const string SaveFileFilter =
        "CppPad Files (*.cpad)|*.cpad|All Files (*.*)|*.*";

    public static readonly string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

    public static readonly string TempFolder = Path.Combine(AppFolder, "Temp");

    public static readonly string TemplateFolder = Path.Combine(AppFolder, "Templates");

    public static readonly string BenchmarkFolder = Path.Combine(AppFolder, "Benchmark");
}