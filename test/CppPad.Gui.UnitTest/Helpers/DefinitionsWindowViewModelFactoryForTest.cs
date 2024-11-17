#region

using CppPad.Gui.ViewModels;
using CppPad.MockFileSystem;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.Gui.UnitTest.Helpers;

public class DefinitionsWindowViewModelFactoryForTest(InMemoryFileSystem fileSystem)
    : IDefinitionsWindowViewModelFactory
{
    public DefinitionsWindowViewModel Create()
    {
        return new DefinitionsWindowViewModel(new DefinitionsViewModel(fileSystem,
            new NullLoggerFactory()));
    }
}