#region

using Avalonia.Controls;

#endregion

namespace CppPad.Gui.Views;

public partial class EditorView : UserControl
{
    public EditorView()
    {
        InitializeComponent();
    }

    private void CompilerOutput_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        textBox.CaretIndex = textBox.Text?.Length ?? 0;
    }
}