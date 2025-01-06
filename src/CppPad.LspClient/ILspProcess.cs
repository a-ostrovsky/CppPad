namespace CppPad.LspClient;

public interface ILspProcess
{
    Task StartAsync();
    Task KillAsync();
    TextReader? OutputReader { get; }
    TextWriter? InputWriter { get; }
    bool HasExited { get; }
}

public class DummyLspProcess : ILspProcess
{
    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task KillAsync()
    {
        return Task.CompletedTask;
    }

    public TextReader? OutputReader => null;

    public TextWriter? InputWriter => null;

    public bool HasExited => false;
}
