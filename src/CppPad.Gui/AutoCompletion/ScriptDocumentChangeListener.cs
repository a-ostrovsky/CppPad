using System;
using System.Collections.Generic;
using CppPad.LspClient.Model;
using CppPad.Scripting;
using Range = CppPad.LspClient.Model.Range;

namespace CppPad.Gui.AutoCompletion;

public class ScriptDocumentChangeListener(Action<IContentUpdate> onContentUpdate)
{
    private ScriptDocument? _scriptDocument = null;

    public void Reset(ScriptDocument scriptDocument)
    {
        _scriptDocument = scriptDocument;
        onContentUpdate(new FullUpdate(scriptDocument));
    }
    
    public void EditText(
        ScriptDocument updatedScriptDocument,
        Range range,
        string? insertedText = null)
    {
        List<IContentUpdate> updates = [];
        if (_scriptDocument != null && insertedText != null &&
            _scriptDocument.Script.Content.Length == insertedText.Length &&
            _scriptDocument.Script.Content == insertedText)
        {
            // No change
        }
        else if (_scriptDocument == null)
        {
            updates.Add(new FullUpdate(updatedScriptDocument));
        }
        else if (insertedText != null)
        {
            updates.Add(new AddTextUpdate(updatedScriptDocument, range, insertedText));
        }
        else
        {
            updates.Add(new RemoveTextUpdate(updatedScriptDocument, range));
        }
        _scriptDocument = updatedScriptDocument;
        foreach (var update in updates)
        {
            onContentUpdate(update);
        }
    }
}