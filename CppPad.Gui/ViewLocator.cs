using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using CppPad.Gui.ViewModels;

namespace CppPad.Gui
{
    public class ViewLocator(IServiceProvider provider) : IDataTemplate
    {
        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                var control = (Control) provider.GetService(type)!;
                return control;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
