using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CppPad.CodeAssistance;
using CppPad.Gui.Eventing;

namespace CppPad.Gui.Observers;

public class CodeAssistanceObserver(ICodeAssistant codeAssistant, EventBus eventBus) : IDisposable
{
    private static readonly TimeSpan UpdateDelay = TimeSpan.FromSeconds(2);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly EventQueue _eventQueue = new();
    private readonly Lock _lock = new();
    private TaskCompletionSource _triggerImmediateSend = new();

    public void Dispose()
    {
        eventBus.UnsubscribeFromEvents(GetType());
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Start()
    {
        eventBus.SubscribeToEvents(GetType(), OnNewEvent);
        ProcessUpdatesInBackground();
    }

    private Task OnNewEvent(IEvent e)
    {
        if (_eventQueue.AddEvent(e) == SendTimePoint.Immediate)
        {
            _triggerImmediateSend.SetResult();
        }

        return Task.CompletedTask;
    }

    private async Task SendEventsAsync()
    {
        var events = _eventQueue.PopEvents();
        foreach (var e in events)
        {
            switch (e)
            {
                case FileClosedEvent fileClosedEvent:
                    await codeAssistant.CloseFileAsync(fileClosedEvent.ScriptDocument);
                    break;
                case FileOpenedEvent fileOpenedEvent:
                    await codeAssistant.OpenFileAsync(fileOpenedEvent.ScriptDocument);
                    break;
                case NewFileEvent newFileEvent:
                    await codeAssistant.OpenFileAsync(newFileEvent.ScriptDocument);
                    break;
                case SettingsChangedEvent settingsChangedEvent:
                    await codeAssistant.UpdateSettingsAsync(settingsChangedEvent.ScriptDocument);
                    break;
                case SourceCodeChangedEvent sourceCodeChangedEvent:
                    await codeAssistant.UpdateContentAsync(sourceCodeChangedEvent.Update);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }
    }

    private void ProcessUpdatesInBackground()
    {
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await SendEventsAsync();
                Task triggerTask;
                lock (_lock)
                {
                    triggerTask = _triggerImmediateSend.Task;
                }

                var waitTask = Task.Delay(UpdateDelay, _cancellationTokenSource.Token);
                await Task.WhenAny(triggerTask, waitTask);
                lock (_lock)
                {
                    _triggerImmediateSend = new TaskCompletionSource();
                }
            }
        });
    }

    private enum SendTimePoint
    {
        Deferred,
        Immediate
    }

    private class EventQueue
    {
        private readonly List<IEvent> _events = [];
        private readonly Lock _lock = new();

        public SendTimePoint AddEvent(IEvent @event)
        {
            var sendTimePoint = @event switch
            {
                SettingsChangedEvent => SendTimePoint.Deferred,
                SourceCodeChangedEvent => SendTimePoint.Deferred,
                _ => SendTimePoint.Immediate
            };
            lock (_lock)
            {
                _events.Add(@event);
            }
            return sendTimePoint;
        }

        public IEvent[] PopEvents()
        {
            lock (_lock)
            {
                var events = _events.ToArray();
                _events.Clear();
                return events;
            }
        }
    }
}