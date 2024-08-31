namespace CppPad.LanguageServer.Interface;

public interface IInstallCallbacks
{
    void OnProgress(string message);

    Task<bool> ConfirmInstallationAsync(string message);
}