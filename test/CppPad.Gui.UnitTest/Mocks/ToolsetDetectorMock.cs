#region

using CppPad.CompilerAdapter.Interface;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class ToolsetDetectorMock : IToolsetDetector
{
    private IReadOnlyList<Toolset> _toolsets = [];

    public Task<IReadOnlyList<Toolset>> GetToolsetsAsync()
    {
        return Task.FromResult(_toolsets);
    }

    public void SetToolsets(ICollection<Toolset> toolsets)
    {
        _toolsets = toolsets.ToList();
    }
}