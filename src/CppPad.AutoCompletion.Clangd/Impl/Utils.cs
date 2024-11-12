namespace CppPad.AutoCompletion.Clangd.Impl;

public static class Utils
{
    public static string PathToUriFormat(string path)
    {
        return $"file:///{path.Replace('\\', '/')}";
    }
}