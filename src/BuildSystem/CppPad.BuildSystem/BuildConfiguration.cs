using CppPad.Scripting;

namespace CppPad.BuildSystem;

public record BuildConfiguration
{
    public required ScriptDocument ScriptDocument { get; init; }

    public required BuildMode BuildMode { get; init; }

    public required EventHandler<ProgressReceivedEventArgs> ProgressReceived { get; init; }

    public required EventHandler<ErrorReceivedEventArgs> ErrorReceived { get; init; }
}
