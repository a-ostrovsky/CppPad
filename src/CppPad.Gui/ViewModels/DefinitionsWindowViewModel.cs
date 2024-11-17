#region

using System;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace CppPad.Gui.ViewModels;

public class DefinitionsWindowViewModel(DefinitionsViewModel definitionsViewModel) : ViewModelBase
{
    public static DefinitionsWindowViewModel DesignInstance { get; } =
        new(DefinitionsViewModel.DesignInstance);

    public DefinitionsViewModel DefinitionsViewModel { get; } = definitionsViewModel;
}

public interface IDefinitionsWindowViewModelFactory
{
    DefinitionsWindowViewModel Create();
}

public class DummyDefinitionsWindowViewModelFactory : IDefinitionsWindowViewModelFactory
{
    public DefinitionsWindowViewModel Create()
    {
        return new DefinitionsWindowViewModel(DefinitionsViewModel.DesignInstance);
    }
}

public class DefinitionsViewModelFactory(IServiceProvider provider)
    : IDefinitionsWindowViewModelFactory
{
    public DefinitionsWindowViewModel Create()
    {
        var vm = provider.GetRequiredService<DefinitionsWindowViewModel>();
        return vm;
    }
}