namespace CppPad.EnvironmentConfiguration.Vs;

public interface IVsWhereAdapter
{
    Task<ICollection<string>> GetVisualStudioPathsAsync();
}