#region

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CppPad.CompilerAdapter.Interface;
using CppPad.Gui.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;

#endregion

namespace CppPad.Gui.ViewModels;

public class ScriptSettingsWindowViewModel : ViewModelBase, IReactiveObject
{
    public ScriptSettingsWindowViewModel(ScriptSettingsViewModel scriptSettings)
    {
        OkCommand = ReactiveCommand.Create(OnOk);
        CancelCommand = ReactiveCommand.Create(OnCancel);
        ScriptSettings = scriptSettings;
    }

    public static ScriptSettingsWindowViewModel DesignInstance { get; } =
        new(new ScriptSettingsViewModel());

    public ReactiveCommand<Unit, Unit> OkCommand { get; }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ScriptSettingsViewModel ScriptSettings { get; }

    public ObservableCollection<CppStandard> CppStandards { get; } = new(
        Enum.GetValues(typeof(CppStandard)).Cast<CppStandard>());

    public ObservableCollection<OptimizationLevel> OptimizationLevels { get; } = new(
        Enum.GetValues(typeof(OptimizationLevel)).Cast<OptimizationLevel>());

    public bool ShouldApplySettings { get; private set; }

    public void RaisePropertyChanging(PropertyChangingEventArgs args)
    {
        this.RaisePropertyChanging(args.PropertyName);
    }

    public void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        this.RaisePropertyChanged(args.PropertyName);
    }

    private void OnOk()
    {
        ShouldApplySettings = true;
        CloseCurrentDialogWindow();
    }

    private void OnCancel()
    {
        ShouldApplySettings = false;
        CloseCurrentDialogWindow();
    }

    private static void CloseCurrentDialogWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
            desktop)
        {
            return;
        }

        desktop.Windows.OfType<ScriptSettingsWindow>().FirstOrDefault()?.Close();
    }
}