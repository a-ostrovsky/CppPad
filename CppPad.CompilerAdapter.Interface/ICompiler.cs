namespace CppPad.CompilerAdapter.Interface;

public interface ICompiler
{
    event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    Task<IExecutable> BuildAsync(Toolset toolset, BuildArgs args);
}

public class DummyCompiler : ICompiler
{
    public event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    public Task<IExecutable> BuildAsync(Toolset toolset, BuildArgs args)
    {
        return Task.FromResult<IExecutable>(new DummyExecutable());
    }
}
