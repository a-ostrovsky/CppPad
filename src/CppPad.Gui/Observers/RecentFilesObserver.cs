using System;
using System.Threading.Tasks;
using CppPad.Configuration;
using CppPad.Gui.Eventing;

namespace CppPad.Gui.Observers;

public class RecentFilesObserver(RecentFiles recentFiles, EventBus eventBus) : IDisposable
{
    public void Dispose()
    {
        eventBus.NewEvent -= EventBus_NewEventAsync;
        GC.SuppressFinalize(this);
    }

    public void Start()
    {
        eventBus.NewEvent += EventBus_NewEventAsync;
    }

    private async Task EventBus_NewEventAsync(object? sender, NewEventEventArgs e)
    {
        switch (e.Event)
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
