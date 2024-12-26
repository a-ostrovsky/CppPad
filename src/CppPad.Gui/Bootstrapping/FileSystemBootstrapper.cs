﻿using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.Bootstrapping;

public class FileSystemBootstrapper(Bootstrapper parent)
{
    public DiskFileSystem FileSystem { get; } = new();
}