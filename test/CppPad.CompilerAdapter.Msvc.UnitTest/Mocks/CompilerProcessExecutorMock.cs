#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Interface;
using CppPad.MockFileSystem;

#endregion

namespace CppPad.CompilerAdapter.Msvc.UnitTest.Mocks;

public class CompilerProcessExecutorMock(InMemoryFileSystem fileSystem) : ICompilerProcessExecutor
{
    private readonly TaskCompletionSource _executionCompletionSource = new();

    public string OutputFileName { get; } = $@"C:\a{Guid.NewGuid()}.exe";
    public event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    public async Task ExecuteAsync(string processName, string args)
    {
        await _executionCompletionSource.Task;
        await fileSystem.WriteAllTextAsync(OutputFileName, string.Empty);
    }

    public void RaiseCompilerMessageReceived(CompilerMessageType type, string message)
    {
        CompilerMessageReceived?.Invoke(this, new CompilerMessageEventArgs(type, message));
    }

    public void SignalExecutionSucceeded()
    {
        _executionCompletionSource.TrySetResult();
    }

    public void SignalExecutionFailed()
    {
        _executionCompletionSource.TrySetException(new CompilationFailedException("Test failure."));
    }
}