using AvaloniaEdit;
using CppPad.Gui.ViewModels;

namespace CppPad.Gui.AutoCompletion;

public interface IAutoCompletionAdapter
{
    void Attach(TextEditor textEditor, SourceCodeViewModel sourceCodeViewModel);
    void Detach();
}

public class DummyAutoCompletionAdapter : IAutoCompletionAdapter
{
    public void Attach(TextEditor textEditor, SourceCodeViewModel sourceCodeViewModel)
    {
    }

    public void Detach()
    {
    }
}