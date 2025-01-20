#region

using CppPad.Gui.AutoCompletion;
using CppPad.LspClient.Model;
using CppPad.Scripting;
using Range = CppPad.LspClient.Model.Range;

#endregion

namespace CppPad.Gui.Tests;

public class AutoCompletionTest
{
    [Fact]
    public void Reset_ShouldTriggerFullUpdate()
    {
        // Arrange
        var updates = new List<IContentUpdate>();
        var listener = new ScriptDocumentChangeListener(u => updates.Add(u));
        var newScriptDocument = new ScriptDocument();

        // Act
        listener.Reset(newScriptDocument);

        // Assert
        Assert.Single(updates);
        Assert.IsType<FullUpdate>(updates[0]);
        Assert.Equal(newScriptDocument, updates[0].ScriptDocument);
    }

    [Fact]
    public void EditText_WithInsertedText_ShouldTriggerAddTextUpdate()
    {
        // Arrange
        var updates = new List<IContentUpdate>();
        var listener = new ScriptDocumentChangeListener(u => updates.Add(u));
        const string initialContent = "Hello";
        const string insertedText = " World";
        const string updatedContent = initialContent + insertedText;
        var initialScript = new ScriptData { Content = initialContent };
        var updatedScript = new ScriptData { Content = updatedContent };
        var initialScriptDocument = new ScriptDocument { Script = initialScript };
        var updatedScriptDocument = new ScriptDocument { Script = updatedScript };
        var range = new Range(
            new Position { Line = 0, Character = 5 },
            new Position { Line = 0, Character = 5 });

        listener.Reset(initialScriptDocument);
        
        updates.Clear();

        // Act
        listener.EditText(updatedScriptDocument, range, insertedText);

        // Assert
        Assert.Single(updates);
        var update = updates[0] as AddTextUpdate;
        Assert.NotNull(update);
        Assert.Equal(updatedScriptDocument, update.ScriptDocument);
        Assert.Equal(range, update.Range);
        Assert.Equal(insertedText, update.Text);
    }

    [Fact]
    public void EditText_WithRemovedText_ShouldTriggerRemoveTextUpdate()
    {
        // Arrange
        var updates = new List<IContentUpdate>();
        var listener = new ScriptDocumentChangeListener(u => updates.Add(u));
        const string initialContent = "Hello World";
        const string updatedContent = "Hello";
        var initialScript = new ScriptData { Content = initialContent };
        var updatedScript = new ScriptData { Content = updatedContent };
        var initialScriptDocument = new ScriptDocument { Script = initialScript };
        var updatedScriptDocument = new ScriptDocument { Script = updatedScript };
        var range = new Range(
            new Position { Line = 0, Character = 5 },
            new Position { Line = 0, Character = 11 });

        listener.Reset(initialScriptDocument);
        updates.Clear();

        // Act
        listener.EditText(updatedScriptDocument, range);

        // Assert
        Assert.Single(updates);
        var update = updates[0] as RemoveTextUpdate;
        Assert.NotNull(update);
        Assert.Equal(updatedScriptDocument, update.ScriptDocument);
        Assert.Equal(range, update.Range);
    }

    [Fact]
    public void EditText_WithNoChange_ShouldNotTriggerUpdate()
    {
        // Arrange
        var updates = new List<IContentUpdate>();
        var listener = new ScriptDocumentChangeListener(u => updates.Add(u));
        const string scriptContent = "Hello World";
        var script = new ScriptData { Content = scriptContent };
        var scriptDocument = new ScriptDocument { Script = script };

        listener.Reset(scriptDocument);
        updates.Clear();

        // Act
        listener.EditText(scriptDocument, new Range(
            new Position{ Line = 0, Character = 0 },
            new Position{ Line = 0, Character = 0 }),
            scriptContent);

        // Assert
        Assert.Empty(updates);
    }

    [Fact]
    public void EditText_WithNullScriptDocument_ShouldTriggerFullUpdate()
    {
        // Arrange
        var updates = new List<IContentUpdate>();
        var listener = new ScriptDocumentChangeListener(u => updates.Add(u));
        var updatedScriptDocument = new ScriptDocument { Script = new ScriptData { Content = "New Content" } };
        var range = new Range(
            new Position { Line = 0, Character = 0 },
            new Position { Line = 0, Character = 0 });
        const string insertedText = "New Content";

        // Act
        listener.EditText(updatedScriptDocument, range, insertedText);

        // Assert
        Assert.Single(updates);
        var update = updates[0] as FullUpdate;
        Assert.NotNull(update);
        Assert.Equal(updatedScriptDocument, update.ScriptDocument);
    }
}