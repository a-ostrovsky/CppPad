﻿namespace CppPad.Common;

public class DummyTimer : ITimer
{
    public void Change(TimeSpan dueTime, TimeSpan period)
    {
        Elapsed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Elapsed;
}

public class Timer : ITimer, IAsyncDisposable
{
    private readonly System.Threading.Timer _timer;

    public Timer()
    {
        _timer = new System.Threading.Timer(Callback);
    }

    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }

    public void Change(TimeSpan dueTime, TimeSpan period)
    {
        _timer.Change(dueTime, period);
    }

    public event EventHandler? Elapsed;

    private void Callback(object? state)
    {
        Elapsed?.Invoke(this, EventArgs.Empty);
    }
}