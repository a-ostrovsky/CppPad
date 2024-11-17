#region

using System.Diagnostics;
using CppPad.Benchmark.Gbench.Interface;
using CppPad.Benchmark.Interface;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.Benchmark.Gbench.Impl;

public class BenchmarkBuilder(
    DiskFileSystem fileSystem,
    ILoggerFactory loggerFactory)
    : IBenchmarkBuilder
{
    private const string CmakeVersion = "3.27.4";

    private readonly ILogger _logger = loggerFactory.CreateLogger<BenchmarkBuilder>();

    public event EventHandler<ProcessOutputReceivedEventArgs>? ProcessOutputReceived;

    public async Task BuildAsync(IInitCallbacks callbacks, string cmakePath, string sourceDir,
        string buildDir, CancellationToken token = default)
    {
        _logger.LogInformation(
            "Building Google Benchmark.... SourceDir: {SourceDir}, BuildDir: {BuildDir}", sourceDir,
            buildDir);

        callbacks.OnNewMessage("Building Google Benchmark....");

        if (!fileSystem.DirectoryExists(buildDir))
        {
            await fileSystem.CreateDirectoryAsync(buildDir);
        }

        var cmakeExecutable = Path.Combine(cmakePath, $"cmake-{CmakeVersion}-windows-x86_64", "bin",
            "cmake.exe");

        callbacks.OnNewMessage($"Cmake executable path: '{cmakeExecutable}'");

        // Configure
        var configureCommand =
            $"{cmakeExecutable} -DCMAKE_BUILD_TYPE=Release -DBENCHMARK_ENABLE_TESTING=OFF -DBENCHMARK_ENABLE_GTEST_TESTS=OFF ..";
        callbacks.OnNewMessage($"Configuring Google Benchmark. Command: {configureCommand}");
        await RunCommandAsync(callbacks, configureCommand, buildDir, token);

        // Build
        var buildCommand = $"{cmakeExecutable} --build . -j 8 --config Release";
        callbacks.OnNewMessage($"Building Google Benchmark. Command: {buildCommand}");
        await RunCommandAsync(callbacks, buildCommand, buildDir, token);

        _logger.LogInformation("Google Benchmark build complete.");
        callbacks.OnNewMessage("Google Benchmark build complete.");
    }

    private async Task RunCommandAsync(IInitCallbacks callbacks, string command,
        string workingDirectory, CancellationToken token = default)
    {
        var processInfo = new ProcessStartInfo("cmd.exe", $"/c \"{command}\"")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true, // Redirect error output
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = processInfo;

        process.OutputDataReceived += (_, a) =>
        {
            if (string.IsNullOrEmpty(a.Data))
            {
                return;
            }

            _logger.LogInformation("VsDevCmd output: {Output}", a.Data);
            callbacks.OnNewMessage($"OUT: {a.Data}");
            ProcessOutputReceived?.Invoke(this, new ProcessOutputReceivedEventArgs(a.Data));
        };

        process.ErrorDataReceived += (_, a) =>
        {
            if (string.IsNullOrEmpty(a.Data))
            {
                return;
            }

            _logger.LogError("VsDevCmd error: {Error}", a.Data);
            callbacks.OnNewMessage($"ERR: {a.Data}");
            ProcessOutputReceived?.Invoke(this, new ProcessOutputReceivedEventArgs(a.Data));
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill();
                _logger.LogWarning("Process was killed due to cancellation.");
            }

            throw;
        }

        if (process.ExitCode != 0)
        {
            throw new BenchmarkException($"Process exited with code {process.ExitCode}: {command}");
        }
    }
}