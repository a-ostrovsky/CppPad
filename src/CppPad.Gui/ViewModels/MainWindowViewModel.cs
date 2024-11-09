#region

using CppPad.Benchmark.Interface;
using CppPad.Common;
using CppPad.Configuration.Interface;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CppPad.AutoCompletion.Interface;
using CppPad.ScriptFile.Interface;

#endregion

namespace CppPad.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase, IReactiveObject
{
    private readonly IConfigurationStore _configurationStore;

    private readonly ObservableAsPropertyHelper<ToolsetViewModel?> _defaultToolset;
    private readonly IEditorViewModelFactory _editorViewModelFactory;
    private readonly ObservableAsPropertyHelper<bool> _hasRecentFiles;
    private readonly AsyncLock _recentFilesLock = new();
    private readonly IRouter _router;
    private EditorViewModel? _currentEditor;

    public MainWindowViewModel(
        ComponentInstallationViewModel componentInstallationViewModel,
        TemplatesViewModel templates,
        IEditorViewModelFactory editorViewModelFactory,
        IRouter router,
        IConfigurationStore configurationStore)
    {
        ComponentInstallation = componentInstallationViewModel;
        Templates = templates;
        _editorViewModelFactory = editorViewModelFactory;
        _router = router;
        _configurationStore = configurationStore;
        NavigateToToolsetEditorCommand =
            ReactiveCommand.CreateFromTask(EditToolsetsAsync);
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
        OpenRecentFileCommand = ReactiveCommand.CreateFromTask<string>(OpenRecentFileAsync);
        ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        CreateNewFileCommand = ReactiveCommand.Create(CreateNewFile);
        CreateNewFileFromTemplateCommand =
            ReactiveCommand.CreateFromTask<string>(CreateNewFileFromTemplateAsync);
        CloseEditorCommand = ReactiveCommand.CreateFromTask<EditorViewModel>(CloseEditorAsync);

        Editors.Add(_editorViewModelFactory.Create());
        CurrentEditor = Editors.FirstOrDefault();

        ReloadToolsets();

        _defaultToolset = Toolsets
            .ToObservableChangeSet()
            .AutoRefreshOnObservable(vm => vm.WhenAnyValue(x => x.IsDefault))
            .Select(_ => Toolsets.FirstOrDefault(x => x.IsDefault))
            .ToProperty(this, x => x.DefaultToolset);

        _hasRecentFiles = this.WhenAnyValue(x => x.RecentFiles.Count)
            .Select(count => count > 0)
            .ToProperty(this, x => x.HasRecentFiles);

        this.ObservableForProperty(x => x.DefaultToolset)
            .Select(change => change.Value)
            .Subscribe(toolset =>
            {
                foreach (var editor in Editors)
                {
                    editor.Toolset = toolset;
                }
            });

        _ = LoadRecentFilesAsync();

        _configurationStore.LastOpenedFileNamesChanged += (s, e) => _ = LoadRecentFilesAsync();
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(
            new ComponentInstallationViewModel(new DummyAutoCompletionInstaller(), new DummyBenchmark(), new DummyRouter(),
                new DummyInstallationProgressWindowViewModelFactory()),
            new TemplatesViewModel(new DummyTemplateLoader()),
            new DummyEditorViewModelFactory(),
            new DummyRouter(),
            new DummyConfigurationStore()
        );

    public ObservableCollection<string> RecentFiles { get; } = [];

    public bool HasRecentFiles => _hasRecentFiles.Value;

    public ReactiveCommand<Unit, Unit> NavigateToToolsetEditorCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

    public ReactiveCommand<string, Unit> OpenRecentFileCommand { get; }

    public ReactiveCommand<Unit, Unit> CreateNewFileCommand { get; }

    public ReactiveCommand<string, Unit> CreateNewFileFromTemplateCommand { get; }

    public ReactiveCommand<EditorViewModel, Unit> CloseEditorCommand { get; }

    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public ObservableCollection<ToolsetViewModel> Toolsets { get; } = [];

    public ObservableCollection<EditorViewModel> Editors { get; } = [];

    public EditorViewModel? CurrentEditor
    {
        get => _currentEditor;
        set => SetProperty(ref _currentEditor, value);
    }

    public TemplatesViewModel Templates { get; }

    public ComponentInstallationViewModel ComponentInstallation { get; }

    public ToolsetViewModel? DefaultToolset => _defaultToolset.Value;

    public void RaisePropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }

    public void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    private Task OpenRecentFileAsync(string filePath)
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var editor = _editorViewModelFactory.Create();
            await editor.LoadSourceCodeAsync(new Uri(filePath, UriKind.Absolute));
            editor.IsModified = false;
            Editors.Add(editor);
            CurrentEditor = editor;
            await _configurationStore.SaveLastOpenedFileNameAsync(filePath);
        });
    }

    private async Task CloseEditorAsync(EditorViewModel editor)
    {
        if (editor.IsModified &&
            await _router.AskUserAsync("Unsaved Changes",
                "You have unsaved changes. Do you want to save before closing?"))
        {
            await editor.SaveAsync();
            if (editor.IsModified) // User canceled save
            {
                return;
            }
        }

        if (!Editors.Contains(editor))
        {
            return;
        }

        Editors.Remove(editor);
        if (CurrentEditor != editor)
        {
            return;
        }

        CurrentEditor = Editors.FirstOrDefault();
    }

    private void CreateNewFile()
    {
        var editor = _editorViewModelFactory.Create();
        Editors.Add(editor);
        CurrentEditor = editor;
    }

    private Task CreateNewFileFromTemplateAsync(string templateName)
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var editor = _editorViewModelFactory.Create();
            await editor.LoadFromTemplateAsync(templateName);
            editor.IsModified = false;
            Editors.Add(editor);
            CurrentEditor = editor;
        });
    }

    private Task OpenFileAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            var uri = await _router.ShowOpenFileDialogAsync(AppConstants.OpenFileFilter);
            if (uri == null)
            {
                return;
            }

            var editor = _editorViewModelFactory.Create();
            await editor.LoadSourceCodeAsync(uri);
            editor.IsModified = false;
            Editors.Add(editor);
            CurrentEditor = editor;
            await _configurationStore.SaveLastOpenedFileNameAsync(uri.LocalPath);
        });
    }

    private void ReloadToolsets()
    {
        var config = _configurationStore.GetToolsetConfiguration();
        Toolsets.Clear();
        foreach (var toolset in config.Toolsets)
        {
            var vm = new ToolsetViewModel(toolset);
            if (config.DefaultToolsetId == toolset.Id)
            {
                vm.IsDefault = true;
                foreach (var editor in Editors)
                {
                    editor.Toolset = vm;
                }
            }

            Toolsets.Add(vm);
        }

        RaisePropertyChanged(
            new PropertyChangedEventArgs(nameof(DefaultToolset)));
    }

    private Task EditToolsetsAsync()
    {
        return ErrorHandler.Instance.RunWithErrorHandlingAsync(async () =>
        {
            await _router.ShowDialogAsync<ToolsetEditorWindowViewModel>();
            ReloadToolsets();
        });
    }

    private async Task LoadRecentFilesAsync()
    {
        using var _ = await _recentFilesLock.LockAsync();

        var recentFiles = await _configurationStore.GetLastOpenedFileNamesAsync();
        RecentFiles.Clear();
        foreach (var fileName in recentFiles)
        {
            RecentFiles.Add(fileName);
        }
    }
}