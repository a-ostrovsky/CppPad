namespace CppPad.CompilerAdapter.Msvc;

public interface IVsWhereAdapter
{
    Task<ICollection<string>> GetVisualStudioPathsAsync();
}