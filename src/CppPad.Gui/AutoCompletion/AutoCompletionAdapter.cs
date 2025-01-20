using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using CppPad.CodeAssistance;
using CppPad.Gui.ViewModels;
using CppPad.LspClient.Model;
using ITimer = CppPad.Common.ITimer;

// TODO: Write unit tests
// TODO: See https://docs.avaloniaui.net/docs/concepts/headless/headless-xunit

namespace CppPad.Gui.AutoCompletion;

public class AutoCompletionAdapter : IAutoCompletionAdapter
{
    private static readonly TimeSpan AutoCompletionUpdateInterval = TimeSpan.FromMilliseconds(1_000);
    private readonly ICodeAssistant _codeAssistant;
    private readonly IDialogs _dialogs;
    private readonly HashSet<char> _triggerCharacters = [];
    private readonly ITimer _updateAutoCompletionTimer;
    private CompletionWindow? _completionWindow;
    private SourceCodeViewModel? _sourceCodeViewModel;
    private TextEditor? _textEditor;

    public AutoCompletionAdapter(
        IDialogs dialogs,
        ICodeAssistant codeAssistant,
        ITimer updateAutoCompletionTimer)
    {
        _dialogs = dialogs;
        _codeAssistant = codeAssistant;
        _updateAutoCompletionTimer = updateAutoCompletionTimer;
        _updateAutoCompletionTimer.Elapsed += UpdateAutoCompletionTimerElapsed;
        RetrieveAndSetServerCapabilitiesInBackground();
    }

    public void Attach(TextEditor textEditor, SourceCodeViewModel sourceCodeViewModel)
    {
        if (_textEditor != null)
        {
            throw new InvalidOperationException("AutoCompletionAdapter is already attached.");
        }

        _sourceCodeViewModel = sourceCodeViewModel;
        _textEditor = textEditor;
        _textEditor.KeyDown += TextEditor_KeyDown;
        _textEditor.TextArea.TextEntered += TextEditor_TextEntered;
    }

    public void Detach()
    {
        if (_textEditor == null)
        {
            throw new InvalidOperationException("AutoCompletionAdapter is not attached.");
        }

        _textEditor.KeyDown -= TextEditor_KeyDown;
        _textEditor.TextArea.TextEntered -= TextEditor_TextEntered;
        _textEditor = null;
    }

    private async void TextEditor_TextEntered(object? sender, TextInputEventArgs e)
    {
        try
        {
            var singleEnteredChar = e.Text is { Length: 1 } ? e.Text[0] : '\0';

            if (_triggerCharacters.Contains(singleEnteredChar))
            {
                if (_completionWindow == null)
                {
                    await ShowCompletionWindowAsync();
                }
                else
                {
                    await UpdateCompletionWindowAsync(_completionWindow);
                }
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(() => _dialogs.NotifyErrorAsync("Failed to show completion window.", ex));
        }
    }

    private async void TextEditor_KeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (e is not { Key: Key.Space, KeyModifiers: KeyModifiers.Control })
            {
                return;
            }

            e.Handled = true;
            await ShowCompletionWindowAsync();
        }
        catch (Exception ex)
        {
            await _dialogs.NotifyErrorAsync("Failed to show completion window.", ex);
        }
    }

    private async Task ShowCompletionWindowAsync()
    {
        if (_textEditor == null)
        {
            throw new InvalidOperationException("AutoCompletionAdapter is not attached.");
        }

        _completionWindow = new CompletionWindow(_textEditor.TextArea)
        {
            Width = 650
        };
        await UpdateCompletionWindowAsync(_completionWindow);

        _completionWindow.Show();
        _completionWindow.Closed += (_, _) =>
        {
            _completionWindow = null;
            _updateAutoCompletionTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        };

        _updateAutoCompletionTimer.Change(AutoCompletionUpdateInterval, Timeout.InfiniteTimeSpan);
    }

    private async Task UpdateCompletionWindowAsync(CompletionWindow completionWindow)
    {
        if (_sourceCodeViewModel == null)
        {
            throw new InvalidOperationException("AutoCompletionAdapter is not attached.");
        }

        var caretOffset = completionWindow.TextArea.Caret.Offset;
        var line = completionWindow.TextArea.Document.GetLineByOffset(caretOffset);
        var lineNumber = line.LineNumber - 1;
        var column = caretOffset - line.Offset;
        var position = new Position { Line = lineNumber, Character = column };

        var data = completionWindow.CompletionList.CompletionData;
        completionWindow.CompletionList.IsFiltering = false;

        var autoCompletions = await _codeAssistant
            .GetCompletionsAsync(_sourceCodeViewModel.ScriptDocument, position);

        if (!HasChanged(completionWindow.CompletionList.CompletionData, autoCompletions))
        {
            return;
        }

        var previouslySelectedItem = completionWindow.CompletionList.SelectedItem;
        data.Clear();
        foreach (var completion in autoCompletions)
        {
            data.Add(new CompletionData(completion));
        }

        var selectedItem = previouslySelectedItem != null
            ? data.FirstOrDefault(d => Equals(d.Text, previouslySelectedItem.Text))
            : null;

        if (selectedItem != null && data.Contains(selectedItem))
        {
            completionWindow.CompletionList.SelectedItem = selectedItem;
        }
        else if (data.Count > 0)
        {
            completionWindow.CompletionList.SelectedItem = data.First();
        }
    }

    private static bool HasChanged(
        ICollection<ICompletionData> completionListCompletionData,
        ICollection<AutoCompletionItem> autoCompletions)
    {
        if (completionListCompletionData.Count != autoCompletions.Count)
        {
            return true;
        }

        var completionListCompletionDataTexts = completionListCompletionData.Select(d => d.Text);
        var autoCompletionsTexts = autoCompletions.Select(d => d.Label);
        return !completionListCompletionDataTexts.SequenceEqual(autoCompletionsTexts);
    }

    private void UpdateAutoCompletionTimerElapsed(object? sender, EventArgs e)
    {
        if (_completionWindow != null)
        {
            Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        if (_completionWindow == null)
                        {
                            return;
                        }

                        await UpdateCompletionWindowAsync(_completionWindow);
                        _updateAutoCompletionTimer.Change(AutoCompletionUpdateInterval, Timeout.InfiniteTimeSpan);
                    }
                    catch (Exception ex)
                    {
                        await _dialogs.NotifyErrorAsync("Failed to update completion window.", ex);
                    }
                },
                DispatcherPriority.Background);
        }
    }

    private void RetrieveAndSetServerCapabilitiesInBackground()
    {
        Task.Run(async () =>
        {
            try
            {
                await RetrieveAndSetServerCapabilitiesAsync();
            }
            catch (Exception e)
            {
                await _dialogs.NotifyErrorAsync("Failed to retrieve server capabilities.", e);
            }
        });
    }

    private async Task RetrieveAndSetServerCapabilitiesAsync()
    {
        var capabilities =
            await _codeAssistant.RetrieveServerCapabilitiesAsync();
        Dispatcher.UIThread.Post(() =>
        {
            _triggerCharacters.Clear();
            foreach (var c in capabilities.TriggerCharacters)
            {
                _triggerCharacters.Add(c);
            }
        }, DispatcherPriority.Background);
    }
}