using CppPad.Gui.ErrorHandling;
using CppPad.Gui.UnitTest.Helpers;

namespace CppPad.Gui.UnitTest;

public abstract class TestBase : IDisposable
{
    protected ObjectTree ObjectTree { get; } = new();

    protected TestBase()
    {
        ErrorHandler.Instance = ObjectTree.ErrorHandler;
    }

    public void Dispose()
    {
        ErrorHandler.Instance = new ErrorHandler();
        GC.SuppressFinalize(this);
    }
}