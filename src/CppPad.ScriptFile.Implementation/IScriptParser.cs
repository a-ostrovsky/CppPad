namespace CppPad.ScriptFile.Implementation;

public interface IScriptParser
{
    ScriptDto Parse(string content);

    ScriptDto FromCppFile(string content);

    string Serialize(ScriptDto scriptDto);
}