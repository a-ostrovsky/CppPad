using CppPad.Gui.Eventing;
using CppPad.Gui.Observers;

namespace CppPad.Gui.Bootstrapping;

public class EventingBootstrapper
{
    public EventingBootstrapper(Bootstrapper parent)
    {
        EventBus = new EventBus(parent.Dialogs);
        RecentFilesObserver = new RecentFilesObserver(
            parent.ConfigurationBootstrapper.RecentFiles,
            EventBus
        );
        CodeAssistanceObserver = new CodeAssistanceObserver(
            parent.CodeAssistanceBootstrapper.CodeAssistant,
            EventBus
        );
    }

    public EventBus EventBus { get; }

    public RecentFilesObserver RecentFilesObserver { get; }

    public CodeAssistanceObserver CodeAssistanceObserver { get; }
}
