using CppPad.CompilerAdapter.Interface;

namespace CppPad.CompilerAdapter.Msvc.Interface;

public interface ICompilerProcessExecutor
{
    event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    Task ExecuteAsync(string processName, string args);
}