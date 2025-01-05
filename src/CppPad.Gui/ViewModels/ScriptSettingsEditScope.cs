using System;
using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class ScriptSettingsEditScope(ScriptSettingsViewModel settings) : IDisposable
{
    private readonly CppBuildSettings _originalSettings = settings.GetCppBuildSettings();

    private bool _isDone;

    public void Dispose()
    {
        if (!_isDone)
        {
            Rollback();
        }

        GC.SuppressFinalize(this);
    }

    public CppBuildSettings Commit()
    {
        if (_isDone)
        {
            throw new InvalidOperationException("Cannot commit more than once.");
        }

        var result = settings.GetCppBuildSettings();
        _isDone = true;
        return result;
    }

    public void Rollback()
    {
        if (_isDone)
        {
            throw new InvalidOperationException("Cannot rollback more than once.");
        }

        settings.ApplySettings(_originalSettings);
        _isDone = true;
    }
}
