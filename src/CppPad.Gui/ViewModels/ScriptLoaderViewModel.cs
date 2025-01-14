using System;
using System.IO;
using System.Threading.Tasks;
using CppPad.Gui.Eventing;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;
using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.ViewModels;

public class ScriptLoaderViewModel(ScriptLoader scriptLoader, EventBus eventBus) : ViewModelBase
{
    public static ScriptLoaderViewModel DesignInstance { get; } =
        new(
            new ScriptLoader(new ScriptSerializer(), new DiskFileSystem()),
            new EventBus(new Dialogs())
        );

    public async Task<ScriptDocument> LoadAsync(string fileName)
    {
        try
        {
            var script = await scriptLoader.LoadAsync(fileName);
            eventBus.Publish(new FileOpenedEvent(script));
            return script;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            eventBus.Publish(new FileOpenedFailedEvent(fileName));
            throw;
        }
    }

    public async Task SaveAsync(ScriptDocument script, string fileName)
    {
        await scriptLoader.SaveAsync(script, fileName);
        eventBus.Publish(new FileSavedEvent(script));
    }
}
