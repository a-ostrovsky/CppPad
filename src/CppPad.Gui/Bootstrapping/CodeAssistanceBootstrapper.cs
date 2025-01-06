using CppPad.CodeAssistance;
using CppPad.LspClient;

namespace CppPad.Gui.Bootstrapping;

public class CodeAssistanceBootstrapper
{
    public CodeAssistanceBootstrapper(Bootstrapper parent)
    {
        LspProcess = new ClangdProcess(
            parent.SystemAdapterBootstrapper.Process,
            parent.SystemAdapterBootstrapper.FileSystem
        );

        LspProxy = new LspProxy(LspProcess);

        ResponseReceiver = new ResponseReceiver(LspProxy);

        RequestSender = new RequestSender(LspProxy);

        CodeAssistant = new CodeAssistant(
            parent.SystemAdapterBootstrapper.FileSystem,
            parent.ScriptingBootstrapper.ScriptLoader,
            ResponseReceiver,
            RequestSender
        );
    }

    public ILspProcess LspProcess { get; }

    public LspProxy LspProxy { get; }

    public ResponseReceiver ResponseReceiver { get; }

    public RequestSender RequestSender { get; }

    public ICodeAssistant CodeAssistant { get; }
}
