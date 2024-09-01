using Avalonia.Data.Core.Plugins;
using CppPad.Common;
using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Impl;
using CppPad.CompilerAdapter.Msvc.Interface;
using CppPad.Configuration.Interface;
using CppPad.Configuration.Json;
using CppPad.FileSystem;
using CppPad.Gui.Routing;
using CppPad.Gui.ViewModels;
using CppPad.Gui.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CppPad.Gui.Bootstrapping;

public class Bootstrapper
{
    public async Task<IServiceProvider> InitializeAsync()
    {
        try
        {
            Directory.CreateDirectory(AppConstants.TempFolder);
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to clear temp folder: {ex}");
            await box.ShowAsync();
            Environment.Exit(1);
        }

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var collection = new ServiceCollection();

        AddCommonServices(collection);
        AddConfigurationServices(collection);
        AddViewModelsAndViews(collection);
        AddCompilerAdapterServices(collection);
        AddStorage(collection);

        var services = collection.BuildServiceProvider();

        return services;
    }

    private static void AddCommonServices(IServiceCollection collection)
    {
        collection.AddCommonServices();
        collection.AddLogging(builder => builder.AddDebug());
    }

    private static void AddConfigurationServices(IServiceCollection collection)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddSingleton<IConfigurationStore, ConfigurationStore>();
        collection.AddSingleton<SettingsFile>();
    }

    private static void AddViewModelsAndViews(IServiceCollection collection)
    {
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddTransient<EditorViewModel>();
        collection.AddTransient<ToolsetEditorWindowViewModel>();

        collection.AddSingleton<MainWindow>();
        collection.AddTransient<EditorView>();
        collection.AddTransient<ToolsetEditorWindow>();

        collection.AddSingleton<IEditorViewModelFactory, EditorViewModelFactory>();
        collection.AddSingleton<IRouter, Router>();
        collection.AddSingleton<ViewLocator>();
    }

    private static void AddCompilerAdapterServices(IServiceCollection collection)
    {
        collection.AddSingleton<ICommandLineBuilder, CommandLineBuilder>();
        collection.AddSingleton<ICompilerProcessExecutor, CompilerProcessExecutor>();
        collection.AddSingleton<IVsWhereAdapter, VsWhereAdapter>();
        collection.AddSingleton<IToolsetDetector, ToolsetDetector>();
        collection.AddSingleton<ICompiler, Compiler>();
    }

    private static void AddStorage(IServiceCollection collection)
    {
        collection.AddSingleton<DiskFileSystem>();
    }
}
