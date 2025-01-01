using System;
using CppPad.SystemAdapter.Execution;
using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.Bootstrapping;

public class SystemAdapterBootstrapper(Bootstrapper parent)
{
    public DiskFileSystem FileSystem { get; } = new();

    public Process Process { get; } = new();
}