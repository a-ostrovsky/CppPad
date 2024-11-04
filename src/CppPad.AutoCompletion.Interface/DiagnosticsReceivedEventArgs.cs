namespace CppPad.AutoCompletion.Interface
{
    public class DiagnosticsReceivedEventArgs(Uri uri, Diagnostic[] diagnostics) : EventArgs
    {
        public Uri Uri { get; } = uri;
        public Diagnostic[] Diagnostics { get; } = diagnostics;
    }
}
