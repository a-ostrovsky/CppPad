namespace CppPad.Common
{
    public class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly Task<IDisposable> _releaser;

        public AsyncLock()
        {
            _releaser = Task.FromResult<IDisposable>(new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted ? _releaser :
                wait.ContinueWith((_, state) => (IDisposable)state!,
                    _releaser.Result, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private void Release()
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }

        private sealed class Releaser(AsyncLock toRelease) : IDisposable
        {
            public void Dispose()
            {
                toRelease.Release();
            }
        }
    }
}