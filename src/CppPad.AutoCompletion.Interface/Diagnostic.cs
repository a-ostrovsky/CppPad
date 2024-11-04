namespace CppPad.AutoCompletion.Interface
{
    public enum DiagnosticSeverity
    {
        Error = 1,
        Warning = 2,
        Information = 3,
        Hint = 4
    }

    public record Diagnostic
    {
        public required Range Range { get; init; }
        public required string Message { get; init; }
        public DiagnosticSeverity Severity { get; init; }
    }
}
