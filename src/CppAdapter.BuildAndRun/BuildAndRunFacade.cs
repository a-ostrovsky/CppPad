namespace CppAdapter.BuildAndRun;

public class BuildAndRunFacade(IBuilder builder, IRunner runner) : IBuildAndRunFacade
{
    public async Task BuildAndRunAsync(
        BuildAndRunConfiguration configuration, CancellationToken token = default)
    {
        try
        {
            builder.BuildStatusChanged += OnBuildStatusChanged;
            var exeFile = await builder.BuildAsync(configuration.BuildConfiguration, token);
            if (exeFile.CreatedFile is null)
            {
                return;
            }

            await runner.RunAsync(
                new RunConfiguration
                {
                    ExecutablePath = exeFile.CreatedFile,
                    Arguments = configuration.ExeArguments,
                    OutputReceived = configuration.ExeOutputReceived,
                    ErrorReceived = configuration.ExeErrorReceived
                },
                token
            );
        }
        finally
        {
            builder.BuildStatusChanged -= OnBuildStatusChanged;
        }
    }

    public event EventHandler<BuildStatusChangedEventArgs>? BuildStatusChanged;

    private void OnBuildStatusChanged(object? sender, BuildStatusChangedEventArgs e)
    {
        BuildStatusChanged?.Invoke(this, e);
    }
}