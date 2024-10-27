#region

using CppPad.Gui.ViewModels;

#endregion

namespace CppPad.Gui.UnitTest.Mocks;

public class
    InstallationProgressWindowViewModelFactoryForTest : IInstallationProgressWindowViewModelFactory
{
    public InstallationProgressWindowViewModel Create()
    {
        return new InstallationProgressWindowViewModel();
    }
}