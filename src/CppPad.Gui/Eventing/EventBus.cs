using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CppPad.Common;

namespace CppPad.Gui.Eventing;

public class EventBus : IDisposable
{
    private readonly ConcurrentQueue<IEvent> _eventQueue = new();
    private readonly Lock _lock = new();
    private readonly ManualResetEventSlim _processingCompleted = new(false);
    private bool _isProcessing;

    public void Publish(IEvent @event)
    {
        _processingCompleted.Reset();
        _eventQueue.Enqueue(@event);
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        lock (_lock)
        {
            if (_isProcessing)
            {
                return;
            }

            _isProcessing = true;
            _processingCompleted.Reset();
        }

        Task.Run(async () =>
        {
            while (_eventQueue.TryDequeue(out var @event))
            {
                if (NewEvent != null)
                {
                    await NewEvent.InvokeAsync(this, new NewEventEventArgs(@event));
                }

                await Task.Yield(); // Ensure other tasks can run
            }

            lock (_lock)
            {
                _isProcessing = false;
                _processingCompleted.Set();
            }
        });
    }

    public event AsyncEventHandler<NewEventEventArgs>? NewEvent;

    public void WaitForProcessing()
    {
        _processingCompleted.Wait();
    }

    public void Dispose()
    {
        _processingCompleted.Wait();
        _processingCompleted.Dispose();
        GC.SuppressFinalize(this);
    }
}
