#region

using CppPad.Gui.ErrorHandling;
using CppPad.Gui.UnitTest.Helpers;

#endregion

namespace CppPad.Gui.UnitTest;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        ErrorHandler.Instance = ObjectTree.ErrorHandler;
    }

    protected ObjectTree ObjectTree { get; } = new();

    public void Dispose()
    {
        ErrorHandler.Instance = new ErrorHandler();
        GC.SuppressFinalize(this);
    }
}