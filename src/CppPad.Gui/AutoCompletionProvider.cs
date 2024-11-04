#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using CppPad.AutoCompletion.Interface;
using AsyncLock = CppPad.Common.AsyncLock;

#endregion

namespace CppPad.Gui;

public class AutoCompletionProvider(IAutoCompletionService autoCompletionService)
{
    private readonly AutoCompletionServiceUpdater _serviceUpdater = new(autoCompletionService);
    private CompletionWindow? _completionWindow;
    private TextEditor? _editor;

    public void Attach(TextEditor editor)
    {
        _editor = editor;
        _editor.KeyDown += OnKeyDown;
        _editor.TextChanged += OnEditorTextChanged;
        _editor.TextArea.TextEntering += OnTextEntering;
        _serviceUpdater.SetText(_editor.Text);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        Debug.Assert(_editor != null);
        _serviceUpdater.SetText(_editor.Text);
    }

    public void Detach()
    {
        if (_editor == null)
        {
            return;
        }

        _editor.KeyDown -= OnKeyDown;
        _editor.TextArea.TextEntering -= OnTextEntering;
        _editor.TextChanged -= OnEditorTextChanged;
        _editor = null;
    }

    public async Task OpenNewFileAsync()
    {
        await _serviceUpdater.OpenOrRenameAsync();
        if (_editor != null)
        {
            _serviceUpdater.SetText(_editor.Text);
        }
    }

    public async Task RenameFileAsync(string fileName)
    {
        await _serviceUpdater.OpenOrRenameAsync(fileName);
        if (_editor != null)
        {
            _serviceUpdater.SetText(_editor.Text);
        }
    }

    private void OnTextEntering(object? sender, TextInputEventArgs e)
    {
        if (!(e.Text?.Length > 0))
        {
            return;
        }
    }

#pragma warning disable VSTHRD100
    private async void OnKeyDown(object? sender, KeyEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (e is { Key: Key.Space, KeyModifiers: KeyModifiers.Control })
        {
            e.Handled = true;
            await ShowCompletionWindowAsync();
        }

        if (e.Key is Key.Enter && _completionWindow != null)
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private async Task ShowCompletionWindowAsync()
    {
        Debug.Assert(_editor != null);
        Debug.Assert(_serviceUpdater.FileName != null);
        await _serviceUpdater.UpdateAsync();
        _completionWindow = new CompletionWindow(_editor.TextArea);
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

        var caretOffset = _editor.TextArea.Caret.Offset;
        var line = _editor.Document.GetLineByOffset(caretOffset);
        var lineNumber = line.LineNumber - 1;
        var column = caretOffset - line.Offset;

        var autoCompletions = await autoCompletionService.GetCompletionsAsync(
            _serviceUpdater.FileName, lineNumber, column);
        foreach (var autoCompletion in autoCompletions)
        {
            data.Add(new MyCompletionData(autoCompletion));
        }

        if (_completionWindow.CompletionList.CompletionData.Count > 0)
        {
            _completionWindow.CompletionList.SelectedItem =
                _completionWindow.CompletionList.CompletionData.First();
        }

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) => { _completionWindow = null; };
    }

    private class AutoCompletionServiceUpdater : IDisposable
    {
        private static readonly TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5);
        private readonly IAutoCompletionService _autoCompletionService;
        private readonly AsyncLock _lock = new();
        private readonly IScheduler _scheduler;
        private IDisposable? _didChangeAction;

        private string? _newText;

        public AutoCompletionServiceUpdater(IAutoCompletionService autoCompletionService,
            IScheduler? scheduler = null)
        {
            _autoCompletionService = autoCompletionService;
            _scheduler = scheduler ?? Scheduler.Default;
            _didChangeAction =
                _scheduler.ScheduleAsync(UpdateDelay, (scheduler, token) => UpdateAsync());
        }

        public string? FileName { get; private set; }

        public void Dispose()
        {
            _didChangeAction?.Dispose();
            _lock.Dispose();
            GC.SuppressFinalize(this);
        }

        public void SetText(string text)
        {
            _newText = text;
        }

        public async Task UpdateAsync()
        {
            using var lck = await _lock.LockAsync();
            _didChangeAction?.Dispose();
            if (FileName != null && _newText != null)
            {
                await _autoCompletionService.DidChangeAsync(FileName, _newText);
                _newText = null;
            }

            _didChangeAction =
                _scheduler.ScheduleAsync(UpdateDelay, (scheduler, token) => UpdateAsync());
        }

        public async Task OpenOrRenameAsync(string? fileName = null)
        {
            using var lck = await _lock.LockAsync();
            fileName ??= $"{Guid.NewGuid()}.cpp";
            if (FileName == null)
            {
                await _autoCompletionService.OpenFileAsync(fileName, string.Empty);
            }
            else
            {
                await _autoCompletionService.RenameFileAsync(FileName, fileName);
            }

            FileName = fileName;
        }
    }
}

public class MyCompletionData(AutoCompletionItem autoCompletionData) : ICompletionData
{
    public string Text { get; } = autoCompletionData.Label;

    public object Content => new TextBlock { Text = Text };

    public object Description => autoCompletionData.Documentation ?? autoCompletionData.Label;

    public double Priority => autoCompletionData.Priority;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        foreach (var edit in autoCompletionData.Edits)
        {
            var startOffset = textArea.Document.GetOffset(edit.Range.Start.Line + 1,
                edit.Range.Start.Character + 1);
            var endOffset =
                textArea.Document.GetOffset(edit.Range.End.Line + 1, edit.Range.End.Character + 1);
            var length = endOffset - startOffset;
            textArea.Document.Replace(startOffset, length, edit.NewText);
        }
    }

    public IImage? Image => null;
}