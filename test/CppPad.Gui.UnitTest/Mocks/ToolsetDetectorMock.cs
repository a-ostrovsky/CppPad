using CppPad.CompilerAdapter.Interface;

namespace CppPad.Gui.UnitTest.Mocks;

public class ToolsetDetectorMock : IToolsetDetector
{
    private IReadOnlyList<Toolset> _toolsets = [];

    public void SetToolsets(ICollection<Toolset> toolsets)
    {
        _toolsets = toolsets.ToList();
    }

    public Task<IReadOnlyList<Toolset>> GetToolsetsAsync()
    {
        return Task.FromResult(_toolsets);
    }
}