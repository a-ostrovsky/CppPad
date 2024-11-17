#region

using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.TextMate;
using CppPad.Gui.ViewModels;
using TextMateSharp.Grammars;

#endregion

namespace CppPad.Gui.Views;

public partial class DefinitionsView : UserControl
{
    private DefinitionsViewModel? ViewModel => DataContext as DefinitionsViewModel;
    
    public DefinitionsView()
    {
        InitializeComponent();
        Init();
    }

    public DefinitionsView(DefinitionsViewModel viewModel)
    {
        InitializeComponent();
        Init();
        DataContext = viewModel;
    }

    private void Init()
    {
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp")
                .Id));
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (ViewModel is not null)
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateViewFromSelectedDefinition();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DefinitionsViewModel.SelectedDefinition))
        {
            UpdateViewFromSelectedDefinition();
        }
    }

    private void UpdateViewFromSelectedDefinition()
    {
        if (ViewModel is null)
        {
            return;
        }
        Editor.Text = ViewModel.SelectedDefinition?.SourceCode ?? string.Empty;
        if (ViewModel.SelectedDefinition != null)
        {
            var line = Editor.Document.GetLineByNumber(ViewModel.SelectedDefinition.Line);
            var lineOffset = line.Offset;
            var startOffset = lineOffset + ViewModel.SelectedDefinition.Character;
            var endOffset = line.EndOffset;
            Editor.CaretOffset = startOffset;
            Editor.SelectionStart = startOffset;
            Editor.SelectionLength = endOffset - startOffset;
            Editor.ScrollToLine(ViewModel.SelectedDefinition.Line);
            
            Editor.Focus();
        }
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateViewFromSelectedDefinition();
    }
}