namespace CppPad.Configuration.Interface;

public record Toolset(
    Guid Id,
    string Type,
    string TargetArchitecture,
    string Name,
    string ExecutablePath);