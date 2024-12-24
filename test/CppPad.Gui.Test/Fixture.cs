using CppPad.Scripting;
using CppPad.UniqueIdentifier;

namespace CppPad.Gui.Tests;

public static class Fixture
{
    public static ScriptDocument CreateScriptDocument()
    {
        var scriptDocument = new ScriptDocument
        {
            Script = new ScriptData
            {
                Content = "int main() { return 0; }",
                BuildSettings = new CppBuildSettings
                {
                    OptimizationLevel = OptimizationLevel.O2,
                    CppStandard = CppStandard.Cpp17
                }
            },
            Identifier = new Identifier("12345"),
            FileName = "s.cpppad"
        };
        return scriptDocument;
    }
}