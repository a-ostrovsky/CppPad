#region

using CppPad.CompilerAdapter.Interface;
using CppPad.CompilerAdapter.Msvc.Interface;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;

#endregion

namespace CppPad.CompilerAdapter.Msvc.Impl;

public class Compiler : ICompiler
{
    private readonly ILogger<Compiler> _logger;
    private readonly DiskFileSystem _fileSystem;
    private readonly ICommandLineBuilder _commandLineBuilder;
    private readonly ICompilerProcessExecutor _compilerProcessExecutor;
    private readonly ILoggerFactory _loggerFactory;

    public Compiler(DiskFileSystem fileSystem,
        ICommandLineBuilder commandLineBuilder,
        ICompilerProcessExecutor compilerProcessExecutor,
        ILoggerFactory loggerFactory)
    {
        _fileSystem = fileSystem;
        _commandLineBuilder = commandLineBuilder;
        _compilerProcessExecutor = compilerProcessExecutor;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<Compiler>();


        _compilerProcessExecutor.CompilerMessageReceived += (_, e) =>
        {
            CompilerMessageReceived?.Invoke(this, e);
        };
    }

    public event EventHandler<CompilerMessageEventArgs>? CompilerMessageReceived;

    public async Task<IExecutable> BuildAsync(Toolset toolset, BuildArgs args)
    {
        _logger.LogInformation("Build process started. Toolset: {toolset}. Args: {args}", toolset, args);
        try
        {
            var tempSourceFilePath = _fileSystem.CreateTempFile("cpp");
            var tempExeFilePath = Path.ChangeExtension(tempSourceFilePath, ".exe");
            var tempBatchFileName = Path.ChangeExtension(tempSourceFilePath, ".bat");

            _logger.LogInformation(
                "Source file created at {TempSourceFilePath}", tempSourceFilePath);
            _logger.LogInformation(
                "Executable file will be created at {TempExeFilePath}", tempExeFilePath);
            _logger.LogInformation(
                "Batch file for compilation will be created at {BatchFilePath}", tempBatchFileName);

            await _fileSystem.WriteAllTextAsync(tempSourceFilePath, args.SourceCode);

            _logger.LogInformation("Source code written to temporary file.");

            var tempBatchFileContent = _commandLineBuilder.BuildBatchFile(toolset, new BuildBatchFileArgs
            {
                SourceFilePath = tempSourceFilePath,
                TargetFilePath = tempExeFilePath,
                AdditionalBuildArgs = args.AdditionalBuildArgs
            });

            await _fileSystem.WriteAllTextAsync(tempBatchFileName, tempBatchFileContent);
            _logger.LogInformation("Batch file created at {TempBatchFile}", tempBatchFileName);
            await _compilerProcessExecutor.ExecuteAsync("cmd.exe", $"/c \"{tempBatchFileName}\"");

            _logger.LogInformation("Compilation succeeded.");

            _fileSystem.DeleteFile(tempSourceFilePath);
            _fileSystem.DeleteFile(tempBatchFileName);

            return new Executable(tempExeFilePath, _fileSystem, _loggerFactory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during compilation.");
            throw new CompilationFailedException(
                "An error occurred during compilation.", ex);
        }
    }
}