namespace CppPad.AutoCompletion.Interface;

public interface IAutoCompletionInstaller
{
    Task InstallAsync(IInitCallbacks initCallbacks, CancellationToken token);
    
    bool IsClangdInstalled();
}

public class DummyAutoCompletionInstaller : IAutoCompletionInstaller
{
    public Task InstallAsync(IInitCallbacks initCallbacks, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public bool IsClangdInstalled()
    {
        return true;
    }
}