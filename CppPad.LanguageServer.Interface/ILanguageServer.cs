namespace CppPad.LanguageServer.Interface
{
    public record AutoCompletionData(string Label, string RemainingText);

    public record Position(int Line, int Character);

    public record FileData(string FileUri, string Content);

    public interface ILanguageServer
    {
        Task<IList<AutoCompletionData>> GetAutoCompletionAsync(FileData fileData, Position position,
            CancellationToken token = default);

        Task InstallAsync(IInstallCallbacks callbacks,
            CancellationToken token = default);
    }

    public class DummyLanguageServer : ILanguageServer
    {
        public Task InstallAsync(IInstallCallbacks callbacks,
            CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task<IList<AutoCompletionData>> GetAutoCompletionAsync(FileData fileData,
            Position position, CancellationToken token = default)
        {
            return Task.FromResult<IList<AutoCompletionData>>([]);
        }
    }
}