#region

using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

#endregion

namespace CppPad.Gui.ViewModels;

public class InstallationProgressWindowViewModel : ViewModelBase, IReactiveObject
{
    private readonly ObservableAsPropertyHelper<bool> _canCancel;
    private readonly Subject<Action?> _onCancelActionSubject = new();
    private int _caretIndex;
    private Action? _onCancelAction;
    private string _statusMessage = string.Empty;

    public InstallationProgressWindowViewModel()
    {
        _canCancel = _onCancelActionSubject
            .Select(action => action != null)
            .ToProperty(this, x => x.CanCancel);
        CancelCommand = ReactiveCommand.Create(
            () => _onCancelAction?.Invoke(),
            _onCancelActionSubject.Select(action => action != null)
        );
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int CaretIndex
    {
        get => _caretIndex;
        set => SetProperty(ref _caretIndex, value);
    }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public bool CanCancel => _canCancel.Value;

    public static InstallationProgressWindowViewModel DesignInstance { get; } = new()
    {
        StatusMessage = "Installing..." + Environment.NewLine + "Line 2" + Environment.NewLine +
                        "Line 3",
        _onCancelAction = () =>
        {
            /* Design-time action */
        }
    };

    public void RaisePropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }

    public void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    public event EventHandler? OnFinished;

    public void Finish()
    {
        OnFinished?.Invoke(this, EventArgs.Empty);
    }

    public void SetOnCancelAction(Action action)
    {
        _onCancelAction = action;
        _onCancelActionSubject.OnNext(action);
    }

    public void AppendStatusMessage(string message)
    {
        StatusMessage += Environment.NewLine + message;
        CaretIndex = StatusMessage.Length;
    }
}

public interface IInstallationProgressWindowViewModelFactory
{
    InstallationProgressWindowViewModel Create();
}

public class
    DummyInstallationProgressWindowViewModelFactory : IInstallationProgressWindowViewModelFactory
{
    public InstallationProgressWindowViewModel Create()
    {
        return InstallationProgressWindowViewModel.DesignInstance;
    }
}

public class InstallationProgressWindowViewModelFactory(IServiceProvider provider)
    : IInstallationProgressWindowViewModelFactory
{
    public InstallationProgressWindowViewModel Create()
    {
        var vm = provider.GetService<InstallationProgressWindowViewModel>()!;
        return vm;
    }
}