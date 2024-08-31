#region

using System.Diagnostics;

using CppPad.CompilerAdapter.Interface;

using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.CompilerAdapter.Msvc;

public class Executable(
    string fileName,
    ILoggerFactory loggerFactory) : IExecutable
{
    private readonly ILogger<Executable> _logger =
        loggerFactory.CreateLogger<Executable>();

    public event EventHandler<OutputReceivedEventArgs>? OutputReceived;
    public event EventHandler<ErrorReceivedEventArgs>? ErrorReceived;
    public event EventHandler<ProcessExitedEventArgs>? ProcessExited;

    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("Starting process: {FileName}", fileName);
            using var process = new Process();

            process.StartInfo.FileName = fileName;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                _logger.LogInformation("Data received: {Data}", e.Data);
                OutputReceived?.Invoke(this,
                    new OutputReceivedEventArgs(e.Data));
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                _logger.LogInformation("Error received: {Data}", e.Data);
                ErrorReceived?.Invoke(this,
                    new ErrorReceivedEventArgs(e.Data));
            };

            process.EnableRaisingEvents = true;

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            _logger.LogInformation(
                "Process exited with code: {ExitCode}", process.ExitCode);
            ProcessExited?.Invoke(this,
                new ProcessExitedEventArgs(process.ExitCode));
        }
        catch (Exception ex)
        {
            ErrorReceived?.Invoke(this,
                new ErrorReceivedEventArgs($"An error occurred: {ex}"));
        }
    }
}
