#region

using CppPad.AutoCompletion.Interface;
using CppPad.ScriptFile.Interface;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.AutoCompletion.Clangd.Impl;

public class ServiceWithInstaller : IAutoCompletionService, IAutoCompletionInstaller
{
    private readonly IAutoCompletionService _autoCompletionService;
    private readonly ClangdInstaller _clangdInstaller;
    private readonly ILogger _logger;
    private bool _isInstalled;

    public ServiceWithInstaller(
        ILoggerFactory loggerFactory,
        IAutoCompletionService autoCompletionService,
        ClangdInstaller clangdInstaller)
    {
        _logger = loggerFactory.CreateLogger<ServiceWithInstaller>();
        _autoCompletionService = autoCompletionService;
        _clangdInstaller = clangdInstaller;
        _isInstalled = IsClangdInstalled();
    }

    public async Task InstallAsync(IInitCallbacks initCallbacks, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Installing Clangd...");
            await _clangdInstaller.InstallAsync(initCallbacks, token);
            _logger.LogInformation("Clangd installed.");
        }
        finally
        {
            Volatile.Write(ref _isInstalled, IsClangdInstalled());
        }
    }

    public bool IsClangdInstalled()
    {
        return _clangdInstaller.IsInstalled();
    }

    public Task OpenFileAsync(string filePath, string fileContent)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.OpenFileAsync(filePath, fileContent);
    }

    public Task CloseFileAsync(string filePath)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.CloseFileAsync(filePath);
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(string filePath, int line, int character)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.FromResult(Array.Empty<AutoCompletionItem>());
        }

        return _autoCompletionService.GetCompletionsAsync(filePath, line, character);
    }

    public Task UpdateSettingsAsync(string filePath, Script script)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.UpdateSettingsAsync(filePath, script);
    }

    public Task UpdateContentAsync(string filePath, string newText)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.UpdateContentAsync(filePath, newText);
    }

    public Task<ServerCapabilities> RetrieveServerCapabilitiesAsync()
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.FromResult(new ServerCapabilities());
        }
        return _autoCompletionService.RetrieveServerCapabilitiesAsync();
    }
}