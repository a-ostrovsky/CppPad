namespace CppPad.BuildSystem.CMakeAdapter.Execution;

public class CMakeExecutionOptions
{
    public required string CMakeListsFolder { get; init; }

    public required string BuildDirectory { get; init; }
    
    public bool ForceConfigure { get; init; }
    
    public EventHandler<ProgressReceivedEventArgs>? ProgressReceived { get; init; }
    
    public EventHandler<ErrorReceivedEventArgs>? ErrorReceived { get; init; }

    public string AdditionalArgsForConfigure { get; init; } = string.Empty;
    
    public string AdditionalArgsForBuild { get; init; } = string.Empty;
}