namespace CppPad.CompilerAdapter.Msvc.Interface;

public interface IVsWhereAdapter
{
    Task<ICollection<string>> GetVisualStudioPathsAsync();
}