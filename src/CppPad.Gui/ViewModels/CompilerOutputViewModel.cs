using System;
using Avalonia.Threading;

namespace CppPad.Gui.ViewModels;

public class CompilerOutputViewModel : ViewModelBase
{
    private string _output = string.Empty;

    public string Output
    {
        get => _output;
        set => SetProperty(ref _output, value);
    }

    public void Reset()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(Reset);
            return;
        }

        Output = string.Empty;
    }

    public void AddMessage(string message)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => AddMessage(message));
            return;
        }

        Output += message + Environment.NewLine;
    }
}
