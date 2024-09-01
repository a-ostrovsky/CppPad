namespace CppPad.ScriptFile.Interface;

public interface IScriptParser
{
    Script Parse(string content);

    Script FromCppFile(string content);

    string Serialize(Script script);
}