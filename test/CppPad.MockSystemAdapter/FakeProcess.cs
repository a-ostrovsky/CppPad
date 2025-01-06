using CppPad.SystemAdapter.Execution;

namespace CppPad.MockSystemAdapter;

public class FakeProcess : Process
{
    private readonly List<StartInfo> _capturedStartInfo = [];
    private EventHandler<DataReceivedEventArgs>? _errorHandler;
    private EventHandler<DataReceivedEventArgs>? _outputHandler;
    private Action<IReadOnlyList<string>> _whenCalled = _ => { };

    public bool StartCalled { get; private set; }

    public IReadOnlyList<StartInfo> CapturedStartInfo => _capturedStartInfo;

    public int ExitCode { get; set; } = 0;

    public override IProcessInfo Start(StartInfo startInfo)
    {
        StartCalled = true;
        _capturedStartInfo.Add(startInfo);

        // Capture the event handlers to trigger them manually in tests.
        _outputHandler = startInfo.OutputReceived;
        _errorHandler = startInfo.ErrorReceived;

        _whenCalled(startInfo.Arguments.AsReadOnly());

        return new MockProcessInfo();
    }

    public override Task<int> WaitForExitAsync(
        IProcessInfo processInfo,
        CancellationToken token = default
    )
    {
        return Task.FromResult(ExitCode);
    }

    public override int WaitForExit(IProcessInfo processInfo)
    {
        return ExitCode;
    }

    public override void Kill(IProcessInfo processInfo)
    {
        // Do nothing.
    }

    public void RaiseOutputData(string data)
    {
        if (_outputHandler != null && !string.IsNullOrEmpty(data))
        {
            var eventArgs = new DataReceivedEventArgs(data);
            _outputHandler.Invoke(this, eventArgs);
        }
    }

    public void RaiseErrorData(string data)
    {
        if (_errorHandler != null && !string.IsNullOrEmpty(data))
        {
            var eventArgs = new DataReceivedEventArgs(data);
            _errorHandler.Invoke(this, eventArgs);
        }
    }

    private class MockProcessInfo : IProcessInfo, IDisposable, IAsyncDisposable
    {
        private readonly MemoryStream _outputStream = new();
        private readonly MemoryStream _errorStream = new();
        private readonly MemoryStream _inputStream = new();

        public bool HasExited { get; set; }

        public object GetProcessData()
        {
            return new object();
        }

        public StreamReader GetStandardOutput()
        {
            return new StreamReader(_outputStream);
        }

        public StreamReader GetStandardError()
        {
            return new StreamReader(_errorStream);
        }

        public StreamWriter GetStandardInput()
        {
            return new StreamWriter(_inputStream);
        }

        public void Dispose()
        {
            _outputStream.Dispose();
            _errorStream.Dispose();
            _inputStream.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await _outputStream.DisposeAsync();
            await _errorStream.DisposeAsync();
            await _inputStream.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    ///     Sets the action to be executed when the process is started.
    /// </summary>
    /// <param name="action">Arguments passed to the process</param>
    public void WhenCalledDo(Action<IReadOnlyList<string>> action)
    {
        _whenCalled = action;
    }
}
