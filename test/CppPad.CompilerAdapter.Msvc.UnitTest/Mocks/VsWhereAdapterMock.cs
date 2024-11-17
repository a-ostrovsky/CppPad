﻿#region

using CppPad.CompilerAdapter.Msvc.Interface;

#endregion

namespace CppPad.CompilerAdapter.Msvc.UnitTest.Mocks;

public class VsWhereAdapterMock : IVsWhereAdapter
{
    private string[] _paths = [];

    public Task<ICollection<string>> GetVisualStudioPathsAsync()
    {
        return Task.FromResult<ICollection<string>>(_paths);
    }

    public void SetVisualStudioPaths(params string[] paths)
    {
        _paths = paths;
    }
}