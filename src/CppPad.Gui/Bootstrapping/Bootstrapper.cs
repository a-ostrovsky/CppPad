namespace CppPad.Gui.Bootstrapping;

public class Bootstrapper
{
    public GuiBootstrapper GuiBootstrapper { get; }
    
    public ScriptingBootstrapper ScriptingBootstrapper { get; }
    
    public FileSystemBootstrapper FileSystemBootstrapper { get; }

    public Bootstrapper()
    {
        FileSystemBootstrapper = new FileSystemBootstrapper(this);
        ScriptingBootstrapper = new ScriptingBootstrapper(this);
        GuiBootstrapper = new GuiBootstrapper(this);
    }
}