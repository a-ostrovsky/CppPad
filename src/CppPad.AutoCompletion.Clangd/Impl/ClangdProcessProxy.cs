using System.Diagnostics;
using System.Text;
using CppPad.AutoCompletion.Clangd.Interface;
using CppPad.Common;

namespace CppPad.AutoCompletion.Clangd.Impl;

public class ClangdProcessProxy : IClangdProcessProxy, IAsyncDisposable
{
    private static readonly string ClangdPath =
        Path.Combine(AppConstants.ClangdFolder, "bin", "clangd.exe");
    
    private readonly Process _process = new()
    {
        StartInfo = new ProcessStartInfo(ClangdPath)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        }
    };
    
    public TextReader? OutputReader { get; private set; }
    
    public TextWriter? InputWriter { get; private set; }
    
    public bool HasExited => _process.HasExited;

    public void Start()
    {
        _process.Start();
        InputWriter = new StreamWriter(_process.StandardInput.BaseStream, new UTF8Encoding(false))
        {
            AutoFlush = true
        };
        OutputReader = new StreamReader(_process.StandardOutput.BaseStream, new UTF8Encoding(false));
    }

    public void Kill()
    {
        _process.Kill();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill();
            _process.Dispose();
        }

        if (InputWriter != null)
        {
            await InputWriter.DisposeAsync();
        }

        OutputReader?.Dispose();

        GC.SuppressFinalize(this);
    }
}