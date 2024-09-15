using CppPad.CompilerAdapter.Interface;

namespace CppPad.CompilerAdapter.Msvc.Interface;

public interface ICommandLineBuilder
{
    string BuildBatchFile(Toolset toolset, BuildBatchFileArgs buildArgs);
}