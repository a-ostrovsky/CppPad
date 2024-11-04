#region

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

#endregion

namespace CppPad.Gui.Views;

public partial class SourceCodeEditorView : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SourceCodeEditorView, string>(nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    private bool _isInternalChange;

    public SourceCodeEditorView()
    {
        InitializeComponent();
        Init();
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public void ScrollToLine(int line)
    {
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        textEditor.ScrollToLine(line);
        textEditor.CaretOffset = GetCaretOffsetForLine(textEditor, line);
    }

    private void Init()
    {
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        var registryOptions = new RegistryOptions(ThemeName.Light);
        var textMateInstallation = textEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cpp")
                .Id));
        textEditor.TextChanged += TextEditor_TextChanged;

        this.GetObservable(TextProperty).Subscribe(text =>
        {
            if (_isInternalChange)
            {
                return;
            }

            textEditor.Text = text;
        });
    }

    public Task SetAutoCompletionProviderAsync(AutoCompletionProvider autoCompletionProvider)
    {
        var textEditor = this.FindControl<TextEditor>("Editor");
        Debug.Assert(textEditor != null);
        autoCompletionProvider.Attach(textEditor);
        return autoCompletionProvider.OpenNewFileAsync();
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        try
        {
            _isInternalChange = true;
            Text = ((TextEditor)sender!).Text;
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
}