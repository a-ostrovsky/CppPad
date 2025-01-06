﻿namespace CppPad.Gui.Bootstrapping;

public class Bootstrapper
{
    public Bootstrapper()
    {
        SystemAdapterBootstrapper = new SystemAdapterBootstrapper(this);
        ConfigurationBootstrapper = new ConfigurationBootstrapper(this);
        ScriptingBootstrapper = new ScriptingBootstrapper(this);
        CodeAssistanceBootstrapper = new CodeAssistanceBootstrapper(this);
        BuildAndRunBootstrapper = new BuildAndRunBootstrapper(this);
        GuiBootstrapper = new GuiBootstrapper(this);
    }

    public CodeAssistanceBootstrapper CodeAssistanceBootstrapper { get; }

    public ConfigurationBootstrapper ConfigurationBootstrapper { get; }

    public GuiBootstrapper GuiBootstrapper { get; }

    public ScriptingBootstrapper ScriptingBootstrapper { get; }

    public SystemAdapterBootstrapper SystemAdapterBootstrapper { get; }

    public BuildAndRunBootstrapper BuildAndRunBootstrapper { get; }
}
