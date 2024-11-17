#region

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CppPad.Gui.ViewModels;
using TextMateSharp.Grammars;

#endregion

namespace CppPad.Gui.Views;

public partial class SourceCodeEditorView : UserControl
{
    private AutoCompletionProvider? _autoCompletionProvider;
    private bool _isInternalChange;

    public SourceCodeEditorView()
    {
        InitializeComponent();
        Init();
    }

    public void ScrollTo(int line, int character)
    {
        Editor.ScrollToLine(line);
        Editor.CaretOffset = GetCaretOffsetForLine(Editor, line) + character;
    }

    private void Init()
    {
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp")
                .Id));

        Editor.TextChanged += TextEditor_TextChanged;
    }

    // Event when setting data context seems not to work with avalonia 11.2??
    public void SetViewModel(EditorViewModel vm)
    {
        DataContext = vm;
        Editor.Text = vm.SourceCode;

        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(EditorViewModel.SourceCode) && !_isInternalChange)
            {
                Editor.Text = vm.SourceCode;
            }
        };

        _autoCompletionProvider = new AutoCompletionProvider(vm);
        _autoCompletionProvider.Attach(Editor);
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

    private static int GetCaretOffsetForLine(TextEditor textEditor, int line)
    {
        var document = textEditor.Document;
        var lineInfo = document.GetLineByNumber(line);
        return lineInfo.Offset;
    }

    public Task ShowDefinitionsAsync()
    {
        return _autoCompletionProvider != null
            ? _autoCompletionProvider.ShowDefinitionsAsync()
            : Task.CompletedTask;
    }
}