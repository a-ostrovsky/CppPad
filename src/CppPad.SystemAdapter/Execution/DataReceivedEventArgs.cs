namespace CppPad.SystemAdapter.Execution;

public class DataReceivedEventArgs(string data) : EventArgs
{
    public string Data => data;

    public static DataReceivedEventArgs From(System.Diagnostics.DataReceivedEventArgs e)
    {
        return new DataReceivedEventArgs(e.Data ?? string.Empty);
    }
}