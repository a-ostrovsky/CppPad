namespace CppPad.CompilerAdapter.Interface;

public interface ICompiler
{
    event EventHandler<CompilerMessageEventArgs>? CompilerMessage;

    Task<IExecutable> BuildAsync(string sourceCode, string additionalBuildArgs);
}

public class DummyCompiler : ICompiler
{
    public event EventHandler<CompilerMessageEventArgs>? CompilerMessage;

    public Task<IExecutable> BuildAsync(string sourceCode, string additionalBuildArgs)
    {
        return Task.FromResult<IExecutable>(new DummyExecutable());
    }
}
