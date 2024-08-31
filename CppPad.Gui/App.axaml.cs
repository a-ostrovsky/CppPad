#region

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc;
using CppPad.Configuration.Interface;
using CppPad.Configuration.Json;
using CppPad.FileSystem;
using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;
using CppPad.Gui.Views;
using CppPad.LanguageServer.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using System;
using System.IO;
using Application = Avalonia.Application;

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
            try
            {
                Directory.CreateDirectory(AppConstants.TempFolder);
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Failed to clear temp folder: " + ex);
                box.ShowAsync().RunSynchronously();
                Environment.Exit(1);
            }

            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var collection = new ServiceCollection();
            collection.AddCommonServices();
            collection.AddLogging(builder => builder.AddDebug());
            collection.AddSingleton<SettingsFile>();
            collection.AddSingleton<IConfiguration>(configuration);
            collection.AddSingleton<IConfigurationStore, ConfigurationStore>();
            collection.AddSingleton<IEditorViewModelFactory, EditorViewModelFactory>();
            collection.AddTransient<EditorViewModel>();
            collection.AddTransient<EditorView>();
            collection.AddSingleton<DiskFileSystem>();
            collection.AddSingleton<MainWindow>();
            collection.AddSingleton<MainWindowViewModel>();
            collection.AddTransient<ToolsetEditorWindow>();
            collection.AddTransient<ToolsetEditorWindowViewModel>();
            collection.AddSingleton<IRouter, Router>();
            collection.AddSingleton<ViewLocator>();
            collection.AddSingleton<IToolsetDetector, ToolsetDetector>();
            collection.AddSingleton<ILanguageServer, LanguageServer.ClangD.LanguageServer>();

            var services = collection.BuildServiceProvider();

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