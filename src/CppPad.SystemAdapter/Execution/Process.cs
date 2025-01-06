using System.Diagnostics;
using CppPad.Logging;
using Microsoft.Extensions.Logging;

namespace CppPad.SystemAdapter.Execution;

public class Process
{
    private readonly ILogger _logger = Log.CreateLogger<Process>();

    public virtual IProcessInfo Start(StartInfo startInfo)
    {
        _logger.LogDebug(
            "Starting process: {FileName} {Arguments}",
            startInfo.FileName,
            startInfo.Arguments
        );
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = startInfo.FileName;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        if (startInfo.Arguments.Count == 1)
        {
            process.StartInfo.Arguments = startInfo.Arguments[0];
        }
        else
        {
            foreach (var argument in startInfo.Arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }
        }

        if (startInfo.OutputReceived != null)
        {
            process.OutputDataReceived += (sender, args) =>
                startInfo.OutputReceived.Invoke(sender, DataReceivedEventArgs.From(args));
        }

        if (startInfo.ErrorReceived != null)
        {
            process.ErrorDataReceived += (sender, args) =>
                startInfo.ErrorReceived.Invoke(sender, DataReceivedEventArgs.From(args));
        }

        if (startInfo.EnvironmentVariables != null)
        {
            foreach (var (key, value) in startInfo.EnvironmentVariables)
            {
                process.StartInfo.EnvironmentVariables[key] = value;
            }
        }

        if (startInfo.AdditionalPaths.Count > 0)
        {
            var pathVariable = process.StartInfo.EnvironmentVariables["PATH"];
            var additionalPaths = string.Join(Path.PathSeparator, startInfo.AdditionalPaths);
            process.StartInfo.EnvironmentVariables["PATH"] =
                $"{pathVariable}{Path.PathSeparator}{additionalPaths}";
        }

        process.Start();
        if (!startInfo.RedirectIoStreams)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        return new ProcessInfo(process);
    }
    
    public virtual void Kill(IProcessInfo processInfo)
    {
        var process = (System.Diagnostics.Process)processInfo.GetProcessData();
        process.Kill();
    }

    public virtual async Task<IDictionary<string, string>> RunAndGetEnvironmentVariablesAsync(
        string executablePath,
        CancellationToken token = default
    )
    {
        var arguments = $"/c \"\"{executablePath}\" & set\"";
        var startInfo = new ProcessStartInfo("cmd.exe", arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using var process = new System.Diagnostics.Process();
        process.StartInfo = startInfo;
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(token);
        var error = await process.StandardError.ReadToEndAsync(token);

        if (!string.IsNullOrEmpty(error))
        {
            throw new ExecutionException($"Error running command: {error}");
        }

        // Parse the environment variables from the 'set' output
        // Each line from `set` typically looks like KEY=VALUE
        var envVars = new Dictionary<string, string>();
        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var index = line.IndexOf('=');
            if (index <= 0)
            {
                continue;
            }

            var key = line[..index].Trim();
            var val = line[(index + 1)..];
            envVars[key] = val;
        }

        return envVars;
    }

    public virtual async Task<int> WaitForExitAsync(
        IProcessInfo processInfo,
        CancellationToken token = default
    )
    {
        int exitCode;
        System.Diagnostics.Process? process = null;
        try
        {
            process = (System.Diagnostics.Process)processInfo.GetProcessData();
            await process.WaitForExitAsync(token);
            exitCode = process.ExitCode;
        }
        finally
        {
            if (token.IsCancellationRequested)
            {
                process?.Kill();
            }
        }

        return exitCode;
    }

    public virtual int WaitForExit(IProcessInfo processInfo)
    {
        var process = (System.Diagnostics.Process)processInfo.GetProcessData();
        process.WaitForExit();
        return process.ExitCode;
    }
}