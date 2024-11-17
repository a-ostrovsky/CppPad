#region

using CppPad.CompilerAdapter.Interface;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Interface;

public interface ICommandLineBuilder
{
    string BuildBatchFile(Toolset toolset, BuildBatchFileArgs buildArgs);
}