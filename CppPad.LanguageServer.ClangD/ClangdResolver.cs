#region

using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.LanguageServer.ClangD
{
    public class ClangdResolver(
        string clangdFolder,
        ILoggerFactory loggerFactory)
    {
        private readonly ILogger _logger =
            loggerFactory.CreateLogger<ClangdResolver>();

        public string? TryGetClangdExecutable()
        {
            if (!Directory.Exists(clangdFolder))
            {
                _logger.LogInformation(
                    "Folder with clangD does not exist. Folder: {ClangDFolder}",
                    clangdFolder);
                return null;
            }

            var clangdFiles = Directory.EnumerateFiles(clangdFolder,
                "clangd.exe", SearchOption.AllDirectories).ToArray();
            if (clangdFiles.Length == 0)
            {
                _logger.LogInformation(
                    "ClangD file does not exist. Folder: {ClangDFolder}",
                    clangdFolder);
                return null;
            }

            if (clangdFiles.Length > 1)
            {
                _logger.LogInformation(
                    "ClangD file does exists multiple times. Folder: {ClangDFolder}",
                    clangdFolder);
                return null;
            }

            clangdFiles = Directory.EnumerateFiles(clangdFolder,
                "clangd.exe", SearchOption.AllDirectories).ToArray();
            _logger.LogInformation(
                "ClangD file found. Folder: {ClangDFolder}, File: {ClangDVersion}",
                clangdFolder, clangdFiles[0]);
            return clangdFiles[0];
        }
    }
}
