using System;

namespace CppPad.Gui.ViewModels;

/// <summary>
///     Creates a new instance of the <see cref="OpenFileRequestedEventArgs" /> class.
/// </summary>
/// <param name="fileName">
///     Name of the file to open. If <see langword="null" />, the user will be prompted to select a file.
/// </param>
public class OpenFileRequestedEventArgs(string? fileName = null) : EventArgs
{
    public string? FileName => fileName;
}
