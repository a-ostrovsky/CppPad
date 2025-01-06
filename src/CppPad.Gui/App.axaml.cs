#region

using System.Reflection.Metadata;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CppPad.Gui.Bootstrapping;
using CppPad.Gui.Views;
using CppPad.LspClient.Model;

#endregion

namespace CppPad.Gui;

public class App : Application
{
    private readonly Bootstrapper _bootstrapper = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _bootstrapper.SystemAdapterBootstrapper.FileSystem.CleanupTemporaryFiles();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _bootstrapper.GuiBootstrapper.MainWindowViewModel,
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}
