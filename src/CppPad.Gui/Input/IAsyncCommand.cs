using System.Threading.Tasks;
using System.Windows.Input;

namespace CppPad.Gui.Input;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object? parameter);
}
