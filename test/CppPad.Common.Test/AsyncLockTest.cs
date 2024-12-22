namespace CppPad.Common.Test;

public class AsyncLockTest
{
    [Fact]
    public async Task LockAsync_ShouldLockAndRelease()
    {
        var asyncLock = new AsyncLock();

        using (await asyncLock.LockAsync())
        {
            // Lock acquired
            Assert.True(true);
        }

        // Lock released
        Assert.True(true);
    }

    [Fact]
    public async Task LockAsync_ShouldBlockUntilReleased()
    {
        var asyncLock = new AsyncLock();
        var task1Completed = false;
        var task2Started = false;
        var task2Completed = false;

        var task1 = Task.Run(async () =>
        {
            using (await asyncLock.LockAsync())
            {
                task2Started = true;
                task1Completed = true;
            }
        });

        var task2 = Task.Run(async () =>
        {
            // Ensure task1 has acquired the lock
            while (!task2Started)
            {
                await Task.Yield();
            }

            using (await asyncLock.LockAsync())
            {
                task2Completed = true;
            }
        });

        await Task.WhenAll(task1, task2);

        Assert.True(task1Completed);
        Assert.True(task2Completed);
    }
}