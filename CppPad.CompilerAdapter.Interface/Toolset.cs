namespace CppPad.CompilerAdapter.Interface;

using ConfigToolset = Configuration.Interface.Toolset;

public record Toolset(string Type, string Name, string ExecutablePath)
{
    public Toolset(ConfigToolset configToolset)
        : this(configToolset.Type,
              configToolset.Name,
              configToolset.ExecutablePath)
    {
    }
}

