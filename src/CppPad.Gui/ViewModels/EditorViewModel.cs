using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CppAdapter.BuildAndRun;
using CppPad.BuildSystem;
using CppPad.Gui.Input;
using CppPad.Scripting;
using CppPad.Scripting.Persistence;
using CppPad.Scripting.Serialization;
using CppPad.SystemAdapter.IO;

namespace CppPad.Gui.ViewModels;

public class EditorViewModel : ViewModelBase, IDisposable
{
    private readonly IBuilder _builder;
    private CancellationTokenSource? _buildCancellationTokenSource;

    private readonly SemaphoreSlim _buildSemaphore = new(1, 1);
    private readonly ScriptLoader _loader;
    private string _title = "Untitled";

    public EditorViewModel(
        ScriptSettingsViewModel scriptSettings,
        ScriptLoader loader,
        IBuilder builder,
        SourceCodeViewModel sourceCode)
    {
        ScriptSettings = scriptSettings;
        _loader = loader;
        _builder = builder;
        SourceCode = sourceCode;
        scriptSettings.ApplySettings(SourceCode.ScriptDocument.Script.BuildSettings);
        CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        _builder.BuildStatusChanged += Builder_BuildStatusChanged;
    }

    private void Builder_BuildStatusChanged(object? sender, BuildStatusChangedEventArgs e)
    {
        var message = e.BuildStatus switch
        {
            BuildStatus.PreparingEnvironment => "Preparing environment...",
            BuildStatus.Building => "Building...",
            BuildStatus.Succeeded => "Build succeeded.",
            BuildStatus.Failed => "Build failed.",
            BuildStatus.Cancelled => "Build cancelled.",
            _ => throw new ArgumentException("Unknown build status.", nameof(e))
        };
        CompilerOutput.AddMessage(message);
    }

    public ScriptSettingsViewModel ScriptSettings { get; }

    public static EditorViewModel DesignInstance { get; } =
        new(
            ScriptSettingsViewModel.DesignInstance,
            new ScriptLoader(new ScriptSerializer(), new DiskFileSystem()),
            new DummyBuilder(),
            SourceCodeViewModel.DesignInstance);

    public SourceCodeViewModel SourceCode { get; }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand CloseCommand { get; }

    public CompilerOutputViewModel CompilerOutput { get; } = new();

    public event EventHandler? CloseRequested;

    public void ApplySettings(CppBuildSettings settings)
    {
        SourceCode.ScriptDocument = SourceCode.ScriptDocument with
        {
            Script = SourceCode.ScriptDocument.Script with
            {
                BuildSettings = settings
            }
        };
    }

    public async Task OpenFileAsync(string fileName)
    {
        var document = await _loader.LoadAsync(fileName);
        SourceCode.ScriptDocument = document;
        Title = Path.GetFileName(document.FileName) ?? "Untitled";
    }

    public async Task SaveFileAsAsync(string fileName)
    {
        await _loader.SaveAsync(SourceCode.ScriptDocument, fileName);
        SourceCode.ScriptDocument = SourceCode.ScriptDocument with { FileName = fileName };
        Title = Path.GetFileName(fileName);
    }

    public Task SaveFileAsync()
    {
        if (string.IsNullOrEmpty(SourceCode.ScriptDocument.FileName))
        {
            throw new InvalidOperationException("File name is not set.");
        }

        return _loader.SaveAsync(SourceCode.ScriptDocument, SourceCode.ScriptDocument.FileName);
    }
    
    public async Task CancelBuildAndRunAsync()
    {
        var cancellationTokenSource = _buildCancellationTokenSource;
        if (cancellationTokenSource is null)
        {
            return;
        }
        
        await cancellationTokenSource.CancelAsync();
    }

    public async Task BuildAndRunAsync(Configuration configuration)
    {
        if (!await _buildSemaphore.WaitAsync(0))
        {
            throw new InvalidOperationException("Another build is already in progress.");
        }
        
        _buildCancellationTokenSource = new CancellationTokenSource();

        try
        {
            CompilerOutput.Reset();
            var buildConfiguration = new BuildConfiguration
            {
                ScriptDocument = SourceCode.ScriptDocument,
                Configuration = configuration,
                ErrorReceived = (_, args) => { CompilerOutput.AddMessage($"ERR:{args.Data}"); },
                ProgressReceived = (_, args) => { CompilerOutput.AddMessage(args.Data); }
            };
            await _builder.BuildAsync(buildConfiguration, _buildCancellationTokenSource.Token);
        }
        finally
        {
            _buildCancellationTokenSource.Dispose();
            _buildCancellationTokenSource = null;
            _buildSemaphore.Release();
        }
    }

    public void Dispose()
    {
        _builder.BuildStatusChanged -= Builder_BuildStatusChanged;
        _buildSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}