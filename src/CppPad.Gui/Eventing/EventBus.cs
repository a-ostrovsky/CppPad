using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CppPad.Gui.Observers;

namespace CppPad.Gui.Eventing;

public class EventBus : IDisposable
{
    private static readonly Type[] AllObservers =
    [
        typeof(CodeAssistanceObserver),
        typeof(RecentFilesObserver),
    ];

    private readonly IDialogs _dialogs;

    private readonly Lock _lock = new();
    private readonly ManualResetEventSlim _processingCompleted = new(false);
    private readonly ConcurrentDictionary<Type, ObserverData> _receivers = new();
    private bool _isProcessing;

    public EventBus(IDialogs dialogs)
    {
        _dialogs = dialogs;
        foreach (var observer in AllObservers)
        {
            _receivers.TryAdd(observer, new ObserverData());
        }
    }

    public void Dispose()
    {
        _processingCompleted.Wait();
        _processingCompleted.Dispose();
        _receivers.Clear();
        GC.SuppressFinalize(this);
    }

    public void Publish(IEvent @event)
    {
        _processingCompleted.Reset();
        foreach (var receiver in _receivers.Values)
        {
            receiver.Events.Enqueue(@event);
        }

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
            foreach (var observerData in _receivers.Values)
            {
                Func<IEvent, Task>? callback;
                lock (_lock)
                {
                    callback = observerData.Callback;
                }

                if (callback == null)
                {
                    continue;
                }

                while (observerData.Events.TryDequeue(out var @event))
                {
                    try
                    {
                        await callback.Invoke(@event);
                        await Task.Yield(); // Ensure other tasks can
                    }
                    catch (Exception e)
                    {
                        await _dialogs.NotifyErrorAsync("Event processing failed.", e);
                    }
                }
            }

            lock (_lock)
            {
                _isProcessing = false;
                _processingCompleted.Set();
            }
        });
    }

    public void UnsubscribeFromEvents(Type caller)
    {
        if (!_receivers.TryGetValue(caller, out _))
        {
            throw new ArgumentException(
                $"Unknown observer. Add observer to {nameof(AllObservers)}",
                nameof(caller)
            );
        }

        lock (_lock)
        {
            _receivers[caller].Callback = null;
        }
    }

    public void SubscribeToEvents(Type caller, Func<IEvent, Task> callback)
    {
        if (!_receivers.TryGetValue(caller, out var receiver))
        {
            throw new ArgumentException(
                $"Unknown observer. Add observer to {nameof(AllObservers)}",
                nameof(caller)
            );
        }

        lock (_lock)
        {
            if (receiver.Callback != null)
            {
                throw new InvalidOperationException("Observer already subscribed to events");
            }

            _receivers[caller].Callback = callback;

            ProcessQueue();
        }
    }

    public void WaitForProcessing()
    {
        _processingCompleted.Wait();
    }

    private class ObserverData
    {
        public Func<IEvent, Task>? Callback { get; set; }

        public ConcurrentQueue<IEvent> Events { get; } = new();
    }
}
