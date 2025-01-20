using CppPad.Scripting;

namespace CppPad.LspClient.Model;

public interface IContentUpdate
{
    ScriptDocument ScriptDocument { get; }
}

public record FullUpdate(ScriptDocument ScriptDocument) : IContentUpdate
{
    public ScriptDocument ScriptDocument { get; } = ScriptDocument;
}

public record AddTextUpdate(ScriptDocument ScriptDocument, Range Range, string Text) : IContentUpdate
{
    public ScriptDocument ScriptDocument { get; } = ScriptDocument;
}

public record RemoveTextUpdate(ScriptDocument ScriptDocument, Range Range) : IContentUpdate
{
    public ScriptDocument ScriptDocument { get; } = ScriptDocument;
}