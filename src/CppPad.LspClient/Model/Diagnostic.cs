namespace CppPad.LspClient.Model;

public record Diagnostic
{
    public required SourceCodeRange Range { get; init; }
    public required string Message { get; init; }
    public DiagnosticSeverity Severity { get; init; }
}
