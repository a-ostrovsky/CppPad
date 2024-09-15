#region

using CppPad.CompilerAdapter.Interface;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Impl;

public class Executable(
    string fileName,
    DiskFileSystem fileSystem,
    ILoggerFactory loggerFactory) : IExecutable
{
    private readonly ILogger<Executable> _logger =
        loggerFactory.CreateLogger<Executable>();

    private string[] _paths = [];

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

            if (_paths.Length > 0)
            {
                var pathVariable = process.StartInfo.EnvironmentVariables["PATH"];
                var additionalPaths = string.Join(Path.PathSeparator, _paths);
                process.StartInfo.EnvironmentVariables["PATH"] = $"{pathVariable}{Path.PathSeparator}{additionalPaths}";
            }

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

            fileSystem.DeleteFile(fileName);
        }
        catch (Exception ex)
        {
            ErrorReceived?.Invoke(this,
                new ErrorReceivedEventArgs($"An error occurred: {ex}"));
        }
    }

    public void SetAdditionalEnvironmentPaths(IEnumerable<string> paths)
    {
        _paths = paths.ToArray();
    }
}