#region

using CppPad.CompilerAdapter.Interface;
using System;
using System.Linq;

#endregion

namespace CppPad.Gui.ViewModels;

public class ScriptSettingsViewModel : ViewModelBase
{
    private string _additionalBuildArgs = string.Empty;
    private string _additionalIncludeDirs = string.Empty;
    private CppStandard _cppStandard = CppStandard.CppLatest;
    private OptimizationLevel _optimizationLevel = OptimizationLevel.Unspecified;
    private string _preBuildCommand = string.Empty;
    private string _librarySearchPaths = string.Empty;
    private string _staticallyLinkedLibraries = string.Empty;
    private string _additionalEnvironmentPaths = string.Empty;

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

    public string[] AdditionalIncludeDirsArray => AdditionalIncludeDirs
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToArray();

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

    public string[] LibrarySearchPathsArray => LibrarySearchPaths
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToArray();

    public string StaticallyLinkedLibraries
    {
        get => _staticallyLinkedLibraries;
        set => SetProperty(ref _staticallyLinkedLibraries, value);
    }

    public string[] StaticallyLinkedLibrariesArray => StaticallyLinkedLibraries
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToArray();

    public string AdditionalEnvironmentPaths
    {
        get => _additionalEnvironmentPaths;
        set => SetProperty(ref _additionalEnvironmentPaths, value);
    }

    public string[] AdditionalEnvironmentPathsArray => AdditionalEnvironmentPaths
        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToArray();
}