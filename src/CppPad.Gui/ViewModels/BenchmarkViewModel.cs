#region

using CppPad.Benchmark.Interface;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.ViewModels;

public class BenchmarkViewModel : ViewModelBase
{
    private readonly IBenchmark _benchmark;

    private readonly IInstallationProgressWindowViewModelFactory
        _installationProgressWindowViewModelFactory;

    private readonly IRouter _router;
    private bool _isInstalling;

    public BenchmarkViewModel(IBenchmark benchmark, IRouter router,
        IInstallationProgressWindowViewModelFactory installationProgressWindowViewModelFactory)
    {
        _benchmark = benchmark;
        _router = router;
        _installationProgressWindowViewModelFactory = installationProgressWindowViewModelFactory;
        InstallCommand = ReactiveCommand.CreateFromTask(InstallBenchmarkAsync,
            this.WhenAnyValue(x => x.IsInstalling).Select(isInstalling => !isInstalling));
    }

    public ReactiveCommand<Unit, Unit> InstallCommand { get; }

    public bool IsInstalling
    {
        get => _isInstalling;
        set => SetProperty(ref _isInstalling, value);
    }

    private async Task InstallBenchmarkAsync()
    {
        var installationProgressWindowViewModel =
            _installationProgressWindowViewModelFactory.Create();
        var installationProgressDialog =
            _router.ShowDialogAsync(installationProgressWindowViewModel);
        var adapter = new InstallationProgressAdapter(_router, installationProgressWindowViewModel);
        var cancellationTokenSource = new CancellationTokenSource();
        installationProgressWindowViewModel.SetOnCancelAction(
            () => cancellationTokenSource.Cancel());
        try
        {
            IsInstalling = true;
            await ErrorHandler.Instance.RunWithErrorHandlingAsync(
                async () =>
                {
                    try
                    {
                        await _benchmark.InitAsync(adapter,
                            new InitSettings { ForceReinstall = true },
                            cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore
                    }
                });
            installationProgressWindowViewModel.Finish();
            await installationProgressDialog;
        }
        finally
        {
            IsInstalling = false;
        }
    }

    private class InstallationProgressAdapter(
        IRouter router,
        InstallationProgressWindowViewModel installationProgressWindowViewModel) : IInitCallbacks
    {
        public Task<bool> AskUserWhetherToInstallAsync(string message)
        {
            return router.AskUserAsync("Benchmark runner is not installed.", message);
        }

        public void OnNewMessage(string message)
        {
            installationProgressWindowViewModel.AppendStatusMessage(message);
        }
    }
}