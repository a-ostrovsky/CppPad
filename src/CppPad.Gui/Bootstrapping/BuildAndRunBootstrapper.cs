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
        VsWhereAdapter = new VsWhereAdapter(parent.SystemAdapterBootstrapper.Process);
        DeveloperCommandPromptDetector = new DeveloperCommandPromptDetector(
            parent.SystemAdapterBootstrapper.FileSystem,
            VsWhereAdapter
        );
        EnvironmentConfigurationDetector = new EnvironmentConfigurationDetector(
            DeveloperCommandPromptDetector,
            parent.SystemAdapterBootstrapper.Process
        );
        EnvironmentConfigurationCache = new EnvironmentConfigurationCache(
            EnvironmentConfigurationDetector
        );
        FileBuilder = new FileBuilder();
        FileWriter = new FileWriter(
            parent.ScriptingBootstrapper.ScriptLoader,
            FileBuilder,
            parent.SystemAdapterBootstrapper.FileSystem
        );
        Executor = new CMakeExecutor(
            parent.SystemAdapterBootstrapper.FileSystem,
            parent.SystemAdapterBootstrapper.Process
        );
        CMake = new CMake(parent.SystemAdapterBootstrapper.FileSystem, FileWriter, Executor);
        Builder = new Builder(EnvironmentConfigurationCache, CMake);
        Runner = new Runner(parent.SystemAdapterBootstrapper.Process);
        BuildAndRunFacade = new BuildAndRunFacade(Builder, Runner);
    }

    public VsWhereAdapter VsWhereAdapter { get; }

    public DeveloperCommandPromptDetector DeveloperCommandPromptDetector { get; }

    public IEnvironmentConfigurationDetector EnvironmentConfigurationDetector { get; }

    public IEnvironmentConfigurationDetector EnvironmentConfigurationCache { get; }

    public FileBuilder FileBuilder { get; }

    public FileWriter FileWriter { get; }

    public CMakeExecutor Executor { get; }

    public CMake CMake { get; }

    public IBuilder Builder { get; }
    
    public IRunner Runner { get; }
    
    public IBuildAndRunFacade BuildAndRunFacade { get; }
}
