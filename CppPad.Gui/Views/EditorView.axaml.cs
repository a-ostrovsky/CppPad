using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CppPad.Gui.ViewModels;
using System;
using System.Diagnostics;
using TextMateSharp.Grammars;

namespace CppPad.Gui.Views;

public partial class EditorView : UserControl
{
    private bool _isInternalChange;

    public EditorView()
    {
        InitializeComponent();
        Init();
    }

    public EditorView(EditorViewModel viewModel)
    {
        InitializeComponent();
        Init();
        DataContext = viewModel;
    }

    private void Init()
    {
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = textEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp").Id));

        textEditor.TextChanged += TextEditor_TextChanged;
        DataContextChanged += EditorView_DataContextChanged;
    }

    private void EditorView_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is null)
        {
            return;
        }
        if (DataContext is not EditorViewModel vm)
        {
            throw new InvalidOperationException("DataContext is not EditorViewModel");
        }
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        vm.PropertyChanged += (_, args) =>
        {
            if (_isInternalChange || args.PropertyName != "SourceCode")
            {
                return;
            }
            textEditor.Text = ((EditorViewModel)DataContext!).SourceCode;
        };
        textEditor.Text = vm.SourceCode;
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        try
        {
            _isInternalChange = true;
            ((EditorViewModel)DataContext!).SourceCode = ((TextEditor)sender!).Text;
        }
        finally
        {
            _isInternalChange = false;
        }
    }
}
