using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CppPad.Gui.Input;
using CppPad.Gui.Views;
using CppPad.Scripting;

namespace CppPad.Gui.ViewModels;

public class ScriptSettingsViewModel : ViewModelBase
{
    private string _additionalBuildArgs = string.Empty;
    private string _additionalEnvironmentPaths = string.Empty;
    private string _additionalIncludeDirs = string.Empty;
    private CppStandard _cppStandard = CppStandard.CppLatest;
    private string _libFiles = string.Empty;
    private string _librarySearchPaths = string.Empty;
    private OptimizationLevel _optimizationLevel = OptimizationLevel.Unspecified;
    private string _preBuildCommand = string.Empty;
    private bool _shouldApplySettings;

    public ScriptSettingsViewModel()
    {
        OkCommand = new RelayCommand(_ => OnOk());
        CancelCommand = new RelayCommand(_ => OnCancel());
    }

    public static ScriptSettingsViewModel DesignInstance { get; } = new();

    public string PreBuildCommand
    {
        get => _preBuildCommand;
        set => SetProperty(ref _preBuildCommand, value);
    }

    public string AdditionalIncludeDirs
    {
        get => _additionalIncludeDirs;
        set => SetProperty(ref _additionalIncludeDirs, value);
    }

    public ObservableCollection<CppStandard> CppStandards { get; } = new(Enum.GetValues<CppStandard>());

    public ObservableCollection<OptimizationLevel> OptimizationLevels { get; } =
        new(Enum.GetValues<OptimizationLevel>());

    public CppStandard CppStandard
    {
        get => _cppStandard;
        set => SetProperty(ref _cppStandard, value);
    }

    public OptimizationLevel OptimizationLevel
    {
        get => _optimizationLevel;
        set => SetProperty(ref _optimizationLevel, value);
    }

    public string AdditionalBuildArgs
    {
        get => _additionalBuildArgs;
        set => SetProperty(ref _additionalBuildArgs, value);
    }

    public string LibrarySearchPaths
    {
        get => _librarySearchPaths;
        set => SetProperty(ref _librarySearchPaths, value);
    }

    public string LibFiles
    {
        get => _libFiles;
        set => SetProperty(ref _libFiles, value);
    }

    public string AdditionalEnvironmentPaths
    {
        get => _additionalEnvironmentPaths;
        set => SetProperty(ref _additionalEnvironmentPaths, value);
    }

    public ICommand OkCommand { get; }

    public ICommand CancelCommand { get; }

    public bool ShouldApplySettings
    {
        get => _shouldApplySettings;
        set => SetProperty(ref _shouldApplySettings, value);
    }

    public ScriptSettingsEditScope StartEdit()
    {
        return new ScriptSettingsEditScope(this);
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

    private static string[] SplitString(string value)
    {
        return value.Split(Environment.NewLine,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public CppBuildSettings GetCppBuildSettings()
    {
        return new CppBuildSettings
        {
            AdditionalIncludeDirs = SplitString(AdditionalIncludeDirs),
            LibSearchPaths = SplitString(LibrarySearchPaths),
            AdditionalEnvironmentPaths = SplitString(AdditionalBuildArgs),
            LibFiles = SplitString(LibFiles),
            CppStandard = CppStandard,
            OptimizationLevel = OptimizationLevel,
            AdditionalBuildArgs = AdditionalBuildArgs,
            PreBuildCommand = PreBuildCommand
        };
    }

    public void ApplySettings(CppBuildSettings settings)
    {
        AdditionalIncludeDirs = string.Join(Environment.NewLine, settings.AdditionalIncludeDirs);
        LibrarySearchPaths = string.Join(Environment.NewLine, settings.LibSearchPaths);
        AdditionalEnvironmentPaths = string.Join(Environment.NewLine, settings.AdditionalEnvironmentPaths);
        AdditionalBuildArgs = settings.AdditionalBuildArgs;
        LibFiles = string.Join(Environment.NewLine, settings.LibFiles);
        CppStandard = settings.CppStandard;
        OptimizationLevel = settings.OptimizationLevel;
        PreBuildCommand = settings.PreBuildCommand;
    }
}