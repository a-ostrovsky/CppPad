using CppPad.LspClient.Model;
using CppPad.Scripting;

namespace CppPad.Gui.Eventing;

public interface IEvent
{
}

public record NewFileEvent(ScriptDocument ScriptDocument) : IEvent;

public record FileOpenedEvent(ScriptDocument ScriptDocument) : IEvent;

public record FileClosedEvent(ScriptDocument ScriptDocument) : IEvent;

public record FileOpenedFailedEvent(string FileName) : IEvent;

public record FileSavedEvent(ScriptDocument ScriptDocument) : IEvent;

public record SourceCodeChangedEvent(IContentUpdate Update, bool FlushNow) : IEvent;

public record SettingsChangedEvent(ScriptDocument ScriptDocument) : IEvent;