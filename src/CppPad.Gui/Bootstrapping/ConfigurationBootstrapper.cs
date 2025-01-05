using CppPad.Configuration;

namespace CppPad.Gui.Bootstrapping;

public class ConfigurationBootstrapper
{
    public ConfigurationBootstrapper(Bootstrapper parent)
    {
        RecentFiles = new RecentFiles(parent.SystemAdapterBootstrapper.FileSystem);
    }

    public RecentFiles RecentFiles { get; }
}