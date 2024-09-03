using System;

namespace CppPad.Gui.ViewModels;

public class GoToLineRequestedEventArgs(int line) : EventArgs
{
    public int Line => line;
}