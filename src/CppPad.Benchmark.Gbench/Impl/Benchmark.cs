#region

using CppPad.Benchmark.Interface;
using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.Benchmark.Gbench.Impl;

public class Benchmark(ILoggerFactory loggerFactory, BenchmarkInstaller installer) : IBenchmark
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Benchmark>();

    public async Task InitAsync(IInitCallbacks callbacks, InitSettings initSettings,
        CancellationToken token = default)
    {
        if (!initSettings.ForceReinstall)
        {
            if (installer.IsBenchmarkInstalled())
            {
                return;
            }

            if (!await callbacks.AskUserWhetherToInstallAsync(
                    "Google Benchmark is not installed. Do you want to install it?"))
            {
                _logger.LogInformation("User skipped installation.");
                return;
            }
        }

        _logger.LogInformation("Installing Google Benchmark");
        await installer.InstallAsync(callbacks, token);
    }

    public Script ToBenchmark(Script script)
    {
        throw new NotImplementedException();
    }
}