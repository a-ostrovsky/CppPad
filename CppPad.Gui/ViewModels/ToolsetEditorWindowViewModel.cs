using CppPad.CompilerAdapter.Interface;
using CppPad.Configuration.Interface;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace CppPad.Gui.ViewModels;

public class ToolsetEditorWindowViewModel : ViewModelBase
{
    public static ToolsetEditorWindowViewModel DesignInstance { get; } =
        new(new DummyToolsetDetector(), new DummyConfigurationStore());

    private readonly IToolsetDetector _toolsetDetector;
    private readonly IConfigurationStore _configurationStore;
    private ToolsetViewModel? _defaultToolset;

    public ObservableCollection<ToolsetViewModel> Toolsets { get; } = [];

    public ReactiveCommand<Unit, Unit> AutodetectToolsetsCommand { get; }

    public ReactiveCommand<ToolsetViewModel, Unit> SetDefaultToolsetCommand { get; }

    public ToolsetEditorWindowViewModel(
        IToolsetDetector toolsetDetector,
        IConfigurationStore configurationStore)
    {
        _toolsetDetector = toolsetDetector;
        _configurationStore = configurationStore;
        var config = _configurationStore.GetToolsetConfiguration();
        foreach (var toolset in config.Toolsets)
        {
            var vm = new ToolsetViewModel(toolset);
            if (config.DefaultToolsetId == toolset.Id)
            {
                vm.IsDefault = true;
                _defaultToolset = vm;
            }
            Toolsets.Add(vm);
        }
        AutodetectToolsetsCommand =
            ReactiveCommand.CreateFromTask(AutodetectToolsetsAsync);
        SetDefaultToolsetCommand =
            ReactiveCommand.CreateFromTask<ToolsetViewModel>(SetDefaultToolsetAsync);
    }

    private async Task AutodetectToolsetsAsync()
    {
        var toolsets = await _toolsetDetector.GetToolsetsAsync();
        Toolsets.Clear();
        foreach (var toolset in toolsets)
        {
            Toolsets.Add(new ToolsetViewModel(toolset));
        }
        if (Toolsets.Count > 0)
        {
            await SetDefaultToolsetAsync(Toolsets[0]);
        }
    }

    public Task SetDefaultToolsetAsync(ToolsetViewModel toolset)
    {
        if (_defaultToolset != null)
        {
            _defaultToolset.IsDefault = false;
        }
        _defaultToolset = toolset;
        _defaultToolset.IsDefault = true;
        return SaveConfigAsync();
    }

    private async Task SaveConfigAsync()
    {
        var config = await _configurationStore.GetToolsetConfigurationAsync();
        config.Toolsets.Clear();
        foreach (var toolset in Toolsets)
        {
            var configToolset = toolset.ToConfigToolset();
            config.Toolsets.Add(configToolset);
            if (toolset.IsDefault)
            {
                config.DefaultToolsetId = configToolset.Id;
            }
        }
        await _configurationStore.SaveToolsetConfigurationAsync(config);
    }
}
