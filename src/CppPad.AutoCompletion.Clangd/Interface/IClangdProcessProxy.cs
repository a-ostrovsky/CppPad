namespace CppPad.AutoCompletion.Clangd.Interface;

public interface IClangdProcessProxy
{
    TextReader? OutputReader { get; }
    TextWriter? InputWriter { get; }
    bool HasExited { get; }
    void Start();
    void Kill();
}