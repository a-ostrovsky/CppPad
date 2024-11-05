#region

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CppPad.Gui.ErrorHandling;
using CppPad.Gui.Routing;
using ReactiveUI;
using AutoCompletionInterface = CppPad.AutoCompletion.Interface;
using BenchmarkInterface = CppPad.Benchmark.Interface;

#endregion

namespace CppPad.Gui.ViewModels;

public class ComponentInstallationViewModel : ViewModelBase
{
    private readonly AutoCompletionInterface.IAutoCompletionInstaller _autoCompletionInstaller;
    private readonly BenchmarkInterface.IBenchmark _benchmark;

    private readonly IInstallationProgressWindowViewModelFactory
        _installationProgressWindowViewModelFactory;

    private readonly IRouter _router;
    private bool _isInstalling;

    public ComponentInstallationViewModel(
        AutoCompletionInterface.IAutoCompletionInstaller autoCompletionInstaller,
        BenchmarkInterface.IBenchmark benchmark,
        IRouter router,
        IInstallationProgressWindowViewModelFactory installationProgressWindowViewModelFactory)
    {
        _autoCompletionInstaller = autoCompletionInstaller;
        _benchmark = benchmark;
        _router = router;
        _installationProgressWindowViewModelFactory = installationProgressWindowViewModelFactory;
        InstallBenchmarkCommand = ReactiveCommand.CreateFromTask(InstallBenchmarkAsync,
            this.WhenAnyValue(x => x.IsInstalling).Select(isInstalling => !isInstalling));
        InstallAutoCompletionCommand = ReactiveCommand.CreateFromTask(InstallAutoCompletionAsync,
            this.WhenAnyValue(x => x.IsInstalling).Select(isInstalling => !isInstalling));
    }

    public ReactiveCommand<Unit, Unit> InstallBenchmarkCommand { get; }

    public ReactiveCommand<Unit, Unit> InstallAutoCompletionCommand { get; }

    public bool IsInstalling
    {
        get => _isInstalling;
        set => SetProperty(ref _isInstalling, value);
    }

    private async Task InstallAutoCompletionAsync()
    {
        var installationProgressWindowViewModel =
            _installationProgressWindowViewModelFactory.Create();
        var installationProgressDialog =
            _router.ShowDialogAsync(installationProgressWindowViewModel);
        var adapter =
            new AutoCompletionInstallationProgressAdapter(installationProgressWindowViewModel);
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
                        await _autoCompletionInstaller.InstallAsync(adapter,
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

    private async Task InstallBenchmarkAsync()
    {
        var installationProgressWindowViewModel =
            _installationProgressWindowViewModelFactory.Create();
        var installationProgressDialog =
            _router.ShowDialogAsync(installationProgressWindowViewModel);
        var adapter =
            new BenchmarkInstallationProgressAdapter(_router, installationProgressWindowViewModel);
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
                        await _benchmark.InitializeAsync(adapter,
                            new BenchmarkInterface.InitSettings { ForceReinstall = true },
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

    private class AutoCompletionInstallationProgressAdapter(
        InstallationProgressWindowViewModel installationProgressWindowViewModel)
        : AutoCompletionInterface.IInitCallbacks
    {
        public void OnNewMessage(string message)
        {
            installationProgressWindowViewModel.AppendStatusMessage(message);
        }
    }

    private class BenchmarkInstallationProgressAdapter(
        IRouter router,
        InstallationProgressWindowViewModel installationProgressWindowViewModel)
        : BenchmarkInterface.IInitCallbacks
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