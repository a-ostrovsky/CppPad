namespace CppPad.Benchmark.Interface;

public interface IInitCallbacks
{
    Task<bool> AskUserWhetherToInstallAsync(string message);

    void OnNewMessage(string message);
}