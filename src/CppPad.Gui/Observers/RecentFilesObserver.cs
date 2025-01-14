using System;
using System.Threading.Tasks;
using CppPad.Configuration;
using CppPad.Gui.Eventing;

namespace CppPad.Gui.Observers;

public class RecentFilesObserver(RecentFiles recentFiles, EventBus eventBus) : IDisposable
{
    public void Start()
    {
        eventBus.SubscribeToEvents(GetType(), OnNewEvent);
    }

    public void Dispose()
    {
        eventBus.UnsubscribeFromEvents(GetType());
        GC.SuppressFinalize(this);
    }

    private async Task OnNewEvent(IEvent e)
    {
        switch (e)
        {
            case FileOpenedEvent fileOpenedEvent:
                await recentFiles.AddAsync(fileOpenedEvent.ScriptDocument.FileName!);
                break;
            case FileSavedEvent fileSavedEvent:
                await recentFiles.AddAsync(fileSavedEvent.ScriptDocument.FileName!);
                break;
            case FileOpenedFailedEvent fileOpenedFailedEvent:
                await recentFiles.RemoveAsync(fileOpenedFailedEvent.FileName);
                break;
        }
    }
}
