namespace CppPad.LspClient;

public interface ILspProcess
{
    TextReader? OutputReader { get; }
    TextReader? ErrorReader { get; }
    TextWriter? InputWriter { get; }
    bool HasExited { get; }
    Task StartAsync();
    Task KillAsync();
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

    public TextReader? ErrorReader => null;

    public bool HasExited => false;
}