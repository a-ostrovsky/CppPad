#region

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

#endregion

namespace CppPad.Gui.Views;

public partial class InputBoxWindow : Window
{
    public InputBoxWindow()
    {
        InitializeComponent();
        Opened += (_, _) => InputTextBox.Focus();
    }

    public string? Result { get; private set; }

    public void SetPrompt(string prompt)
    {
        PromptTextBlock.Text = prompt;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Result = InputTextBox.Text;
        // Add logic to evaluate the input if needed
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = null;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                OkButton_Click(sender, e);
                break;
            case Key.Escape:
                CancelButton_Click(sender, e);
                break;
        }
    }
}