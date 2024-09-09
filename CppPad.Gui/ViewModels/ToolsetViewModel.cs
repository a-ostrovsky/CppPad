#region

using System;
using CompilerToolset = CppPad.CompilerAdapter.Interface.Toolset;
using ConfigToolset = CppPad.Configuration.Interface.Toolset;
using CpuArchitecture = CppPad.CompilerAdapter.Interface.CpuArchitecture;

#endregion

namespace CppPad.Gui.ViewModels;

public class ToolsetViewModel : ViewModelBase
{
    private readonly Guid _id = Guid.Empty;
    private string _executablePath = string.Empty;

    private bool _isDefault;
    private string _name = string.Empty;
    private CpuArchitecture _targetArchitecture;
    private string _type = string.Empty;

    public ToolsetViewModel()
    {
    }

    public ToolsetViewModel(CompilerToolset toolset)
    {
        Type = toolset.Type;
        Name = toolset.Name;
        TargetArchitecture = toolset.TargetArchitecture;
        ExecutablePath = toolset.ExecutablePath;
    }

    public ToolsetViewModel(ConfigToolset toolset)
    {
        Type = toolset.Type;
        Name = toolset.Name;
        ExecutablePath = toolset.ExecutablePath;
        if (Enum.TryParse<CpuArchitecture>(toolset.TargetArchitecture, out var targetArchitecture))
        {
            TargetArchitecture = targetArchitecture;
        }

        _id = toolset.Id;
    }

    public static ToolsetViewModel DesignInstance { get; } = new()
    {
        Type = "Type",
        Name = "Name",
        ExecutablePath = "ExecutablePath"
    };

    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string ExecutablePath
    {
        get => _executablePath;
        set => SetProperty(ref _executablePath, value);
    }

    public CpuArchitecture TargetArchitecture
    {
        get => _targetArchitecture;
        set => SetProperty(ref _targetArchitecture, value);
    }

    public ConfigToolset ToConfigToolset()
    {
        return new ConfigToolset(
            _id == Guid.Empty ? Guid.NewGuid() : _id,
            Type,
            TargetArchitecture.ToString(),
            Name,
            ExecutablePath
        );
    }

    public CompilerToolset ToCompilerToolset()
    {
        return new CompilerToolset(Type, TargetArchitecture, Name, ExecutablePath);
    }
}