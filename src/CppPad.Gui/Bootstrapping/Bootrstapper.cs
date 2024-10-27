#region

using Avalonia.Data.Core.Plugins;
using CppPad.Benchmark.Gbench.Impl;
using CppPad.Benchmark.Gbench.Interface;
using CppPad.Benchmark.Interface;
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
using CppPad.ScriptFile.Interface;
using CppPad.ScriptFile.Json;
using CppPad.ScriptFileLoader.Interface;
using CppPad.ScriptFileLoader.OnFileSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using System;
using System.IO;
using System.Threading.Tasks;

#endregion

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
            var box = MessageBoxManager.GetMessageBoxStandard("Error",
                $"Failed to clear temp folder: {ex}");
            await box.ShowAsync();
            Environment.Exit(1);
        }

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var collection = new ServiceCollection();

        AddCommonServices(collection);
        AddConfigurationServices(collection);
        AddGui(collection);
        AddCompilerAdapterServices(collection);
        AddStorage(collection);
        AddScriptFileHandling(collection);
        AddBenchmark(collection);

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
            .AddJsonFile("appsettings.json", false, true)
            .Build();
        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddSingleton<IConfigurationStore, ConfigurationStore>();
        collection.AddSingleton<SettingsFile>();
    }

    private static void AddGui(IServiceCollection collection)
    {
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddTransient<EditorViewModel>();
        collection.AddTransient<ToolsetEditorWindowViewModel>();
        collection.AddTransient<ScriptSettingsWindowViewModel>();
        collection.AddSingleton<TemplatesViewModel>();
        collection.AddSingleton<BenchmarkViewModel>();
        collection.AddTransient<InstallationProgressWindowViewModel>();

        collection.AddSingleton<MainWindow>();
        collection.AddTransient<EditorView>();
        collection.AddTransient<ToolsetEditorWindow>();
        collection.AddTransient<ScriptSettingsWindow>();
        collection.AddTransient<InstallationProgressWindow>();

        collection.AddSingleton<IEditorViewModelFactory, EditorViewModelFactory>();
        collection
            .AddSingleton<IInstallationProgressWindowViewModelFactory,
                InstallationProgressWindowViewModelFactory>();
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

    private static void AddScriptFileHandling(IServiceCollection collection)
    {
        collection.AddSingleton<IScriptParser, ScriptParser>();
        collection.AddSingleton<IScriptLoader, ScriptLoader>();
        collection.AddSingleton<ITemplateLoader, TemplateLoader>();
    }

    private static void AddBenchmark(IServiceCollection collection)
    {
        collection.AddSingleton<IBenchmark, Benchmark.Gbench.Impl.Benchmark>();
        collection.AddSingleton<BenchmarkInstaller>();
        collection.AddSingleton<IBenchmarkBuilder, BenchmarkBuilder>();
        collection.AddSingleton<IBenchmarkDownloader, HttpBenchmarkDownloader>();
    }
}