using CppPad.SystemAdapter.Execution;
using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.Bootstrapping;

#pragma warning disable CS9113 // Parameter is unread. For consistency.
public class SystemAdapterBootstrapper(Bootstrapper parent)
#pragma warning restore CS9113 // Parameter is unread.
{
    public DiskFileSystem FileSystem { get; } = new();

    public Process Process { get; } = new();
}
