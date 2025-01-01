using CppAdapter.BuildAndRun;
using CppPad.BuildSystem.CMakeAdapter;
using CppPad.BuildSystem.CMakeAdapter.Creation;
using CppPad.BuildSystem.CMakeAdapter.Execution;
using CppPad.EnvironmentConfiguration;
using CppPad.EnvironmentConfiguration.Vs;

namespace CppPad.Gui.Bootstrapping;

public class BuildAndRunBootstrapper
{
    public BuildAndRunBootstrapper(Bootstrapper parent)
    {
        VsWhereAdapter = new VsWhereAdapter(
            parent.SystemAdapterBootstrapper.Process);
        DeveloperCommandPromptDetector = new DeveloperCommandPromptDetector(
            parent.SystemAdapterBootstrapper.FileSystem,
            VsWhereAdapter);
        EnvironmentConfigurationDetector = new EnvironmentConfigurationDetector(
            DeveloperCommandPromptDetector,
            parent.SystemAdapterBootstrapper.Process);
        FileBuilder = new FileBuilder();
        Executor = new CMakeExecutor(
            parent.SystemAdapterBootstrapper.FileSystem,
            parent.SystemAdapterBootstrapper.Process);
        CMake = new CMake(
            parent.SystemAdapterBootstrapper.FileSystem,
            parent.ScriptingBootstrapper.ScriptLoader,
            FileBuilder,
            Executor);
        Builder = new Builder(EnvironmentConfigurationDetector, CMake);
    }
    
    public VsWhereAdapter VsWhereAdapter {get;}
    
    public DeveloperCommandPromptDetector DeveloperCommandPromptDetector {get;}
    
    public IEnvironmentConfigurationDetector EnvironmentConfigurationDetector { get; }

    public FileBuilder FileBuilder { get; }

    public CMakeExecutor Executor { get; }

    public CMake CMake { get; }
    
    public IBuilder Builder { get; }
}