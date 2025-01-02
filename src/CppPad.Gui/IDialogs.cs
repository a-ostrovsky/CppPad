using System;
using System.Threading.Tasks;
using CppPad.Gui.ViewModels;

namespace CppPad.Gui;

public interface IDialogs
{
    Task NotifyErrorAsync(string message, Exception? exception = null);
    void NotifyError(string message, Exception? exception = null);
    Task<string?> ShowFileOpenDialogAsync(string filter);
    Task<string?> ShowFileSaveDialogAsync(string filter);
    Task<string?> InputBoxAsync(string prompt, string title, string defaultResponse = "");
    Task ShowScriptSettingsDialogAsync(ScriptSettingsViewModel viewModel);
}