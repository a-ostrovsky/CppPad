﻿using CppAdapter.BuildAndRun;
using CppPad.Configuration;
using CppPad.Gui.Eventing;
using CppPad.Gui.Observers;
using CppPad.Gui.Tests.Fakes;
using CppPad.Gui.ViewModels;
using CppPad.MockSystemAdapter;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.Tests;

public class Bootstrapper : IDisposable
{
    public Bootstrapper()
    {
        Runner = new FakeRunner();
        EventBus = new EventBus();
        RecentFiles = new RecentFiles(FileSystem);
        BuildAndRunFacade = new BuildAndRunFacade(Builder, Runner);
        ScriptSerializer = new ScriptSerializer();
        ScriptLoader = new ScriptLoader(ScriptSerializer, FileSystem);
        RecentFilesObserver = new RecentFilesObserver(RecentFiles, EventBus);
        ScriptLoaderViewModel = new ScriptLoaderViewModel(ScriptLoader, EventBus);
        OpenEditorsViewModel = new OpenEditorsViewModel(CreateEditorViewModel);
        ToolbarViewModel = new ToolbarViewModel(RecentFiles);
        ScriptSettingsViewModel = new ScriptSettingsViewModel();
        MainWindowViewModel = new MainWindowViewModel(
            OpenEditorsViewModel,
            ToolbarViewModel,
            Dialogs
        );

        RecentFilesObserver.Start();
    }

    public EventBus EventBus { get; }

    public RecentFilesObserver RecentFilesObserver { get; }

    public ScriptSerializer ScriptSerializer { get; }

    public BuildAndRunFacade BuildAndRunFacade { get; }

    public FakeDialogs Dialogs { get; } = new();

    public InMemoryFileSystem FileSystem { get; } = new();

    public RecentFiles RecentFiles { get; }

    public FakeBuilder Builder { get; } = new();

    public FakeRunner Runner { get; }

    public ScriptLoader ScriptLoader { get; }

    public MainWindowViewModel MainWindowViewModel { get; }

    public ScriptSettingsViewModel ScriptSettingsViewModel { get; }

    public OpenEditorsViewModel OpenEditorsViewModel { get; }

    public ToolbarViewModel ToolbarViewModel { get; }

    public ScriptLoaderViewModel ScriptLoaderViewModel { get; }

    private EditorViewModel CreateEditorViewModel()
    {
        return new EditorViewModel(
            ScriptSettingsViewModel,
            ScriptLoaderViewModel,
            BuildAndRunFacade,
            new SourceCodeViewModel()
        );
    }

    public void Dispose()
    {
        MainWindowViewModel.Dispose();
        RecentFilesObserver.Dispose();
        GC.SuppressFinalize(this);
    }
}
