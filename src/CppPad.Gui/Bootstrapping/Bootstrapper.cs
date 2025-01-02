namespace CppPad.Gui.Bootstrapping;

public class Bootstrapper
{
    public Bootstrapper()
    {
        SystemAdapterBootstrapper = new SystemAdapterBootstrapper(this);
        ScriptingBootstrapper = new ScriptingBootstrapper(this);
        BuildAndRunBootstrapper = new BuildAndRunBootstrapper(this);
        GuiBootstrapper = new GuiBootstrapper(this);
    }

    public GuiBootstrapper GuiBootstrapper { get; }

    public ScriptingBootstrapper ScriptingBootstrapper { get; }

    public SystemAdapterBootstrapper SystemAdapterBootstrapper { get; }

    public BuildAndRunBootstrapper BuildAndRunBootstrapper { get; }
}