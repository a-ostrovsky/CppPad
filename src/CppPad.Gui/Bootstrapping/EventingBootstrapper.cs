using CppPad.Gui.Eventing;
using CppPad.Gui.Observers;

namespace CppPad.Gui.Bootstrapping;

public class EventingBootstrapper
{
    public EventingBootstrapper(Bootstrapper parent)
    {
        EventBus = new EventBus();
        RecentFilesObserver = new RecentFilesObserver(
            parent.ConfigurationBootstrapper.RecentFiles,
            EventBus
        );
    }

    public EventBus EventBus { get; }

    public RecentFilesObserver RecentFilesObserver { get; }
}
