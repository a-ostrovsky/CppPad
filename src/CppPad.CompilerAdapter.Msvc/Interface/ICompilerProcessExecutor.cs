#region

using CppPad.CompilerAdapter.Interface;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Interface;

public interface ICompilerProcessExecutor
{
    event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    Task ExecuteAsync(string processName, string args);
}