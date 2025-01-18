using System;
using CppPad.LspClient.Model;

namespace CppPad.Gui.ViewModels;

public class ContentUpdatedEventArgs(IContentUpdate contentUpdate) : EventArgs
{
    public IContentUpdate ContentUpdate => contentUpdate;
}