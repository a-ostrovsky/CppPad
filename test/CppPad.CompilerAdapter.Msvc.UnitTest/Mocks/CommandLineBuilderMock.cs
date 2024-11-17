#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Interface;

#endregion

namespace CppPad.CompilerAdapter.Msvc.UnitTest.Mocks;

public class CommandLineBuilderMock : ICommandLineBuilder
{
    public string BuildBatchFile(Toolset toolset, BuildBatchFileArgs buildArgs)
    {
        return "cl.exe --dummy";
    }
}