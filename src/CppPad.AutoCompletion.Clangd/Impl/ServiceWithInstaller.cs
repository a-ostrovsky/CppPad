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

    public Task OpenFileAsync(ScriptDocument scriptDocument)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.OpenFileAsync(scriptDocument);
    }

    public Task CloseFileAsync(ScriptDocument document)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.CloseFileAsync(document);
    }

    public Task<AutoCompletionItem[]> GetCompletionsAsync(ScriptDocument document, Position position)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.FromResult(Array.Empty<AutoCompletionItem>());
        }

        return _autoCompletionService.GetCompletionsAsync(document, position);
    }

    public Task UpdateSettingsAsync(ScriptDocument document)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.UpdateSettingsAsync(document);
    }

    public Task UpdateContentAsync(ScriptDocument document)
    {
        if (!Volatile.Read(ref _isInstalled))
        {
            _logger.LogWarning("Clangd is not installed. No auto completion is possible.");
            return Task.CompletedTask;
        }

        return _autoCompletionService.UpdateContentAsync(document);
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