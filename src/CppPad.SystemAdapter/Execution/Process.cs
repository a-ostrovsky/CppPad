namespace CppPad.SystemAdapter.Execution;

public class Process
{
    public virtual IProcessInfo Start(StartInfo startInfo)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = startInfo.FileName;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        foreach (var argument in startInfo.Arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.OutputDataReceived += (sender, args) =>
            startInfo.OutputReceived.Invoke(sender, DataReceivedEventArgs.From(args));
        process.ErrorDataReceived += (sender, args) =>
            startInfo.ErrorReceived.Invoke(sender, DataReceivedEventArgs.From(args));

        if (startInfo.AdditionalPaths.Count > 0)
        {
            var pathVariable = process.StartInfo.EnvironmentVariables["PATH"];
            var additionalPaths = string.Join(Path.PathSeparator, startInfo.AdditionalPaths);
            process.StartInfo.EnvironmentVariables["PATH"] =
                $"{pathVariable}{Path.PathSeparator}{additionalPaths}";
        }

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return new ProcessInfo(process);
    }

    public virtual async Task<int> WaitForExitAsync(IProcessInfo processInfo,
        CancellationToken cancellationToken = default)
    {
        var process = (System.Diagnostics.Process)processInfo.GetProcessData();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    public virtual int WaitForExit(IProcessInfo processInfo)
    {
        var process = (System.Diagnostics.Process)processInfo.GetProcessData();
        process.WaitForExit();
        return process.ExitCode;
    }
}