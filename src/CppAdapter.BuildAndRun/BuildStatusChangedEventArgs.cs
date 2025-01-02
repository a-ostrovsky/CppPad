namespace CppAdapter.BuildAndRun;

public class BuildStatusChangedEventArgs(BuildStatus buildStatus) : EventArgs
{
    public BuildStatus BuildStatus => buildStatus;
}