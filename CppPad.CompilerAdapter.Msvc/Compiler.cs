using System.Diagnostics;

using CppPad.CompilerAdapter.Interface;
using CppPad.FileSystem;

using Microsoft.Extensions.Logging;

namespace CppPad.CompilerAdapter.Msvc;

public class Compiler : ICompiler
{
    private readonly string _clExePath;
    private readonly DiskFileSystem _fileSystem;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Compiler> _logger;

    public Compiler(
        string clExePath,
        DiskFileSystem fileSystem,
        ILoggerFactory loggerFactory)
    {
        if (!fileSystem.Exists(clExePath))
        {
            throw new ArgumentException($"File '{clExePath}' does not exist.");
        }
        _clExePath = clExePath;
        _fileSystem = fileSystem;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<Compiler>();
    }

    public event EventHandler<CompilerMessageEventArgs>? CompilerMessage;

    public async Task<IExecutable> BuildAsync(
        string sourceCode, string additionalBuildArgs)
    {
        _logger.LogInformation("Build process started.");
        try
        {
            var tempSourceFile = Path.GetTempFileName() + ".cpp";
            var tempExeFile = Path.ChangeExtension(tempSourceFile, ".exe");
            var tempBatchFile = Path.ChangeExtension(tempSourceFile, ".bat");

            _logger.LogInformation(
                "Source file created at {TempSourceFile}", tempSourceFile);
            _logger.LogInformation(
                "Executable file will be created at {TempExeFile}", tempExeFile);
            _logger.LogInformation(
                "Batch file for compilation will be created at {BatchFile}", tempBatchFile);

            await _fileSystem.WriteAllTextAsync(tempSourceFile, sourceCode);

            _logger.LogInformation("Source code written to temporary file.");

            // Determine the path to vcvarsall.bat based on the cl.exe path
            var clExeDirectory = Path.GetDirectoryName(_clExePath)!;
            var vcvarsallPath = Path.Combine(
                clExeDirectory, @"..\..\..\..\..\..\Auxiliary\Build\vcvarsall.bat");
            vcvarsallPath = Path.GetFullPath(vcvarsallPath);

            var batchContent = $@"
@echo off
call ""{vcvarsallPath}"" x64
cl.exe ""{tempSourceFile}"" /Fe""{tempExeFile}"" {additionalBuildArgs}
";

            await _fileSystem.WriteAllTextAsync(tempBatchFile, batchContent);
            _logger.LogInformation("Batch file created at {TempBatchFile}", tempBatchFile);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{tempBatchFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                {
                    return;
                }
                _logger.LogInformation("Compiler output: {Output}", args.Data);
                CompilerMessage?.Invoke(this, new CompilerMessageEventArgs(
                    CompilerMessageType.Info, args.Data));
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                {
                    return;
                }
                _logger.LogError("Compiler error/warning: {Error}", args.Data);
                var messageType = args.Data.Contains(" warning ")
                    ? CompilerMessageType.Warning
                    : CompilerMessageType.Error;
                CompilerMessage?.Invoke(this,
                    new CompilerMessageEventArgs(CompilerMessageType.Error, args.Data));
            };

            process.Start();
            _logger.LogInformation("Compilation process started.");
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Compilation failed with exit code {ExitCode}.",
                    process.ExitCode);
                throw new CompilationFailedException();
            }

            _logger.LogInformation("Compilation succeeded.");
            return new Executable(tempExeFile, _loggerFactory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during compilation.");
            throw new CompilationFailedException(
                "An error occurred during compilation.", ex);
        }
    }
}
