#region

using CppPad.Common;
using CppPad.Configuration.Interface;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using CppPad.ScriptFileLoader.Interface;
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

#endregion

namespace CppPad.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase, IReactiveObject
{
    private readonly IConfigurationStore _configurationStore;

    private readonly ObservableAsPropertyHelper<ToolsetViewModel?> _defaultToolset;
    private readonly IEditorViewModelFactory _editorViewModelFactory;
    private readonly IRouter _router;
    private EditorViewModel? _currentEditor;
    private string _progressMessage = string.Empty;
    private bool _shouldShowProgressDialog;

    public MainWindowViewModel(
        TemplatesViewModel templates,
        IEditorViewModelFactory editorViewModelFactory,
        IRouter router,
        IConfigurationStore configurationStore)
    {
        Templates = templates;
        _editorViewModelFactory = editorViewModelFactory;
        _router = router;
        _configurationStore = configurationStore;
        NavigateToToolsetEditorCommand =
            ReactiveCommand.CreateFromTask(EditToolsetsAsync);
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
        ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        CreateNewFileCommand = ReactiveCommand.Create(CreateNewFile);
        CreateNewFileFromTemplateCommand = ReactiveCommand.CreateFromTask<string>(CreateNewFileFromTemplateAsync);
        CloseEditorCommand = ReactiveCommand.Create<EditorViewModel>(CloseEditor);

        Editors.Add(_editorViewModelFactory.Create());
        CurrentEditor = Editors.FirstOrDefault();

        ReloadToolsets();

        _defaultToolset = Toolsets
            .ToObservableChangeSet()
            .AutoRefreshOnObservable(vm => vm.WhenAnyValue(x => x.IsDefault))
            .Select(_ => Toolsets.FirstOrDefault(x => x.IsDefault))
            .ToProperty(this, x => x.DefaultToolset);

        this.ObservableForProperty(x => x.DefaultToolset)
            .Select(change => change.Value)
            .Subscribe(toolset =>
            {
                foreach (var editor in Editors)
                {
                    editor.Toolset = toolset;
                }
            });
    }

    public static MainWindowViewModel DesignInstance { get; } =
        new(
            new TemplatesViewModel(new DummyTemplateLoader()),
            new DummyEditorViewModelFactory(),
            new DummyRouter(),
            new DummyConfigurationStore()
        );

    public ReactiveCommand<Unit, Unit> NavigateToToolsetEditorCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

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

    public bool ShouldShowProgressDialog
    {
        get => _shouldShowProgressDialog;
        set => SetProperty(ref _shouldShowProgressDialog, value);
    }

    public string ProgressMessage
    {
        get => _progressMessage;
        set => SetProperty(ref _progressMessage, value);
    }

    public TemplatesViewModel Templates { get; }

    public ToolsetViewModel? DefaultToolset => _defaultToolset.Value;

    public void RaisePropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }

    public void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    private void CloseEditor(EditorViewModel editor)
    {
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
            Editors.Add(editor);
            CurrentEditor = editor;
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
}