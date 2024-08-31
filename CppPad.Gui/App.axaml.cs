#region

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CppPad.Common;
using CppPad.Gui.Bootstrapping;
using CppPad.Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

#endregion

namespace CppPad.Gui;

public class App : Application
{
    private ILogger? _logger;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new Bootstrapper().Initialize();

            var window = services.GetRequiredService<MainWindow>();
            DataTemplates.Add(services.GetRequiredService<ViewLocator>());

            _logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<App>();

            desktop.MainWindow = window;
            desktop.Exit += Desktop_Exit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        try
        {
            Directory.Delete(AppConstants.TempFolder);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete temp folder.");
        }
    }
}