#region

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CppPad.Common;
using CppPad.Gui.Bootstrapping;
using CppPad.Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;

#endregion

namespace CppPad.Gui;

public class App : Application
{
    private ILogger? _logger;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private class ObservableExceptionHandler : IObserver<Exception>
    {
        public void OnNext(Exception value)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            RxApp.MainThreadScheduler.Schedule(() => throw value);
        }

        public void OnError(Exception error)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            RxApp.MainThreadScheduler.Schedule(() => throw error);
        }

        public void OnCompleted()
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            RxApp.MainThreadScheduler.Schedule(() => throw new NotSupportedException());
        }
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        try
        {
            RxApp.DefaultExceptionHandler = new ObservableExceptionHandler();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = await Bootstrapper.InitializeAsync();

                var window = services.GetRequiredService<MainWindow>();
                // TODO: Do we need this?
                //DataTemplates.Add(services.GetRequiredService<ViewLocator>());

                _logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<App>();

                desktop.MainWindow = window;
                desktop.Exit += Desktop_Exit;
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "Failed to initialize application.");
            throw;
        }
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        try
        {
            Directory.Delete(AppConstants.TempFolder, true);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete temp folder.");
        }
    }
}