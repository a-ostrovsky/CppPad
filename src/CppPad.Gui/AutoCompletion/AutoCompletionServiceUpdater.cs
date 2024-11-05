#region

using System;
using System.Threading;
using System.Threading.Tasks;
using CppPad.AutoCompletion.Interface;
using CppPad.Common;
using ITimer = CppPad.Common.ITimer;

#endregion

namespace CppPad.Gui.AutoCompletion;

public class AutoCompletionServiceUpdater : IDisposable
{
    private static readonly TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5);
    private readonly IAutoCompletionService _autoCompletionService;
    private readonly AsyncLock _lock = new();
    private readonly ITimer _timer;
    private string? _newText;

    public string FileIdentifier { get; } = $"{Guid.NewGuid()}.cpp";

    public AutoCompletionServiceUpdater(IAutoCompletionService autoCompletionService, ITimer timer)
    {
        _autoCompletionService = autoCompletionService;
        _timer = timer;
        _timer.Elapsed += TimerOnElapsed;
        _timer.Change(UpdateDelay, Timeout.InfiniteTimeSpan);
    }

    private async void TimerOnElapsed(object? sender, EventArgs e)
    {
        await UpdateAsync();
    }

    public void Dispose()
    {
        _lock.Dispose();
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        GC.SuppressFinalize(this);
    }

    public void SetText(string text)
    {
        _newText = text;
    }

    public async Task UpdateAsync()
    {
        using var lck = await _lock.LockAsync();
        if (_newText != null)
        {
            await _autoCompletionService.DidChangeAsync(FileIdentifier, _newText);
            _newText = null;
        }

        _timer.Change(UpdateDelay, Timeout.InfiniteTimeSpan);
    }

    public async Task OpenOrRenameAsync(string? fileName = null)
    {
        using var lck = await _lock.LockAsync();
        await _autoCompletionService.OpenFileAsync(FileIdentifier, string.Empty);
    }
}