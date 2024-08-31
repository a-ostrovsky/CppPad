namespace CppPad.LanguageServer.ClangD
{
    public class ProgressEventArgs(string message) : EventArgs
    {
        public string Message { get; } = message;
    }
}
