namespace CppPad.Common;

public class AsyncLock : IDisposable
{
    private readonly Task<IDisposable> _releaser;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AsyncLock()
    {
        _releaser = Task.FromResult<IDisposable>(new Releaser(this));
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task<IDisposable> LockAsync()
    {
        var wait = _semaphore.WaitAsync();
        return wait.IsCompleted
            ? _releaser
            : wait.ContinueWith((_, state) => (IDisposable)state!,
                _releaser.Result, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private void Release()
    {
        _semaphore.Release();
    }

    private sealed class Releaser(AsyncLock toRelease) : IDisposable
    {
        public void Dispose()
        {
            toRelease.Release();
        }
    }
}