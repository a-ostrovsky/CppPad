#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Interface;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Impl;

public class CompilerProcessExecutor(ILoggerFactory loggerFactory) : ICompilerProcessExecutor
{
    private readonly ILogger _logger =
        loggerFactory.CreateLogger<CompilerProcessExecutor>();

    public event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    public async Task ExecuteAsync(string processName, string args)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = processName,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = processStartInfo };

        process.OutputDataReceived += (_, a) =>
        {
            if (string.IsNullOrEmpty(a.Data))
            {
                return;
            }

            _logger.LogInformation("Compiler output: {Output}", a.Data);
            CompilerMessageReceived?.Invoke(this, new CompilerMessageEventArgs(
                CompilerMessageType.Info, a.Data));
        };

        process.ErrorDataReceived += (_, a) =>
        {
            if (string.IsNullOrEmpty(a.Data))
            {
                return;
            }

            _logger.LogError("Compiler error/warning: {Error}", a.Data);
            var messageType = a.Data.Contains(" warning ")
                ? CompilerMessageType.Warning
                : CompilerMessageType.Error;
            CompilerMessageReceived?.Invoke(this,
                new CompilerMessageEventArgs(messageType, a.Data));
        };

        process.Start();
        _logger.LogInformation("Compilation process started.");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("Compilation failed with exit code {ExitCode}.",
                process.ExitCode);
            throw new CompilationFailedException();
        }
    }
}