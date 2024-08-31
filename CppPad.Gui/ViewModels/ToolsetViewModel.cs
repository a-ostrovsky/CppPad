using CppPad.CompilerAdapter.Interface;

using ConfigToolset = CppPad.Configuration.Interface.Toolset;
using CompilerToolset = CppPad.CompilerAdapter.Interface.Toolset;
using CppPad.Gui.Views;
using System;

namespace CppPad.Gui.ViewModels;

public class ToolsetViewModel : ViewModelBase
{
    public static ToolsetViewModel DesignInstance { get; } = new()
    {
        Type = "Type",
        Name = "Name",
        ExecutablePath = "ExecutablePath"
    };

    private bool _isDefault = false;
    private string _name = string.Empty;
    private string _executablePath = string.Empty;
    private string _type = string.Empty;
    private readonly Guid _id = Guid.Empty;

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

    public ToolsetViewModel()
    {

    }

    public ToolsetViewModel(CompilerToolset toolset)
    {
        Type = toolset.Type;
        Name = toolset.Name;
        ExecutablePath = toolset.ExecutablePath;
    }

    public ToolsetViewModel(ConfigToolset toolset)
    {
        Type = toolset.Type;
        Name = toolset.Name;
        ExecutablePath = toolset.ExecutablePath;
        _id = toolset.Id;
    }

    public ConfigToolset ToConfigToolset()
    {
        return new(
            _id == Guid.Empty ? Guid.NewGuid() : _id,
            Type,
            Name,
            ExecutablePath
        );
    }

    public CompilerToolset ToCompilerToolset()
    {
        return new CompilerToolset(Type, Name, ExecutablePath);
    }
}
