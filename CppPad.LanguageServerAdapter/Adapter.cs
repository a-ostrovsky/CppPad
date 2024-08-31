using CppPad.LanguageServerAdapter.Responses;
using Microsoft.Extensions.Logging;

namespace CppPad.LanguageServerAdapter;

public class Adapter(LanguageServerWorkflow workflow, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Adapter>();

    private readonly Dictionary<string, int> _openDocumentsToVersion = new(StringComparer.OrdinalIgnoreCase);

    public async Task StartAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Starting adapter.");
        var response = await workflow.InitializeAsync(token);
        _logger.LogInformation("Adapter started.");
        if (response.Result.Capabilities.CompletionProvider == null)
        {
            throw new LanguageServerAdapterException("Completion provider is not available");
        }
    }

    public Task OpenOrUpdateFile(string uri, string content,
        CancellationToken token = default)
    {
        return _openDocumentsToVersion.ContainsKey(uri)
            ? NotifyFileChangeAsync(uri, content, token)
            : OpenFileAsync(uri, content, token);
    }

    public async Task OpenFileAsync(string uri, string content, CancellationToken token = default)
    {
        _logger.LogInformation("Starting completion.");
        const int version = 1;
        _openDocumentsToVersion[uri] = version;
        await workflow.DidOpenAsync(uri, content, version, token);
    }

    public async Task NotifyFileChangeAsync(string uri, string content, CancellationToken token = default)
    {
        _logger.LogInformation("Starting completion.");
        var newVersion = _openDocumentsToVersion[uri]++;
        await workflow.DidChangeAsync(uri, content, newVersion, token);
    }

    public async Task CloseFileAsync(string uri, CancellationToken token = default)
    {
        _logger.LogInformation("Stopping completion.");
        await workflow.DidCloseAsync(uri, token);
        _openDocumentsToVersion.Remove(uri);
    }

    public async Task<CompletionItem[]> RequestCompletionAsync(
        string uri, int line, int character, CancellationToken token = default)
    {
        _logger.LogInformation("Requesting completion.");
        if (!_openDocumentsToVersion.ContainsKey(uri))
        {
            throw new LanguageServerAdapterException("The document is not open");
        }
        var response = await workflow.RequestCompletionAsync(uri, line, character, token);
        return response.Result.Items;
    }

    public async Task StopAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Stopping adapter.");
        foreach (var uri in _openDocumentsToVersion.Keys)
        {
            await workflow.DidCloseAsync(uri, token);
        }
        await workflow.ShutdownAsync(token);
        _logger.LogInformation("Adapter stopped.");
    }
}