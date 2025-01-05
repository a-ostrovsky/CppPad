using System;
using System.IO;
using System.Threading.Tasks;
using CppPad.Configuration;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;
using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.ViewModels;

public class ScriptLoaderViewModel(ScriptLoader scriptLoader, RecentFiles recentFiles)
    : ViewModelBase
{
    public static ScriptLoaderViewModel DesignInstance { get; } =
        new(
            new ScriptLoader(new ScriptSerializer(), new DiskFileSystem()),
            new RecentFiles(new DiskFileSystem())
        );

    public async Task<ScriptDocument> LoadAsync(string fileName)
    {
        try
        {
            var script = await scriptLoader.LoadAsync(fileName);
            await recentFiles.AddAsync(fileName);
            return script;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await recentFiles.RemoveAsync(fileName);
            throw;
        }
    }

    public async Task SaveAsync(ScriptDocument script, string fileName)
    {
        await scriptLoader.SaveAsync(script, fileName);
        await recentFiles.AddAsync(fileName);
    }
}
