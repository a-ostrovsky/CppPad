using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;

namespace CppPad.Gui.Bootstrapping;

public class ScriptingBootstrapper
{
    public ScriptingBootstrapper(Bootstrapper parent)
    {
        ScriptSerializer = new ScriptSerializer();
        ScriptLoader = new ScriptLoader(
            ScriptSerializer,
            parent.SystemAdapterBootstrapper.FileSystem);
    }

    public ScriptSerializer ScriptSerializer { get; }

    public ScriptLoader ScriptLoader { get; }
}