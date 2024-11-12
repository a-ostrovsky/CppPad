namespace CppPad.AutoCompletion.Clangd.Interface;

public interface IClangdProcessProxy
{
    StreamReader? OutputReader { get; }
    StreamWriter? InputWriter { get; }
    bool HasExited { get; }
    void Start();
    void Kill();
}