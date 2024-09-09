namespace CppPad.CompilerAdapter.Interface;

#region

using ConfigToolset = Configuration.Interface.Toolset;

#endregion

public record Toolset(
    string Type,
    CpuArchitecture TargetArchitecture,
    string Name,
    string ExecutablePath)
{
    public Toolset(ConfigToolset configToolset)
        : this(configToolset.Type,
            Enum.Parse<CpuArchitecture>(configToolset.TargetArchitecture),
            configToolset.Name,
            configToolset.ExecutablePath)
    {
    }
}