#region

using CppPad.ScriptFile.Interface;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

#endregion

namespace CppPad.Gui.ViewModels;

public class TemplatesViewModel : ViewModelBase
{
    private readonly ITemplateLoader _templateLoader;
    private bool _hasTemplates;

    public TemplatesViewModel(ITemplateLoader templateLoader)
    {
        _templateLoader = templateLoader;
        RefreshAsync().ContinueWith(_ =>
        {
            _templateLoader.TemplatesChanged += async (_, _) => await RefreshAsync();
        });
        DeleteCommand = ReactiveCommand.Create<string>(Delete);
    }

    public ObservableCollection<string> TemplateFileNames { get; } = [];

    public ReactiveCommand<string, Unit> DeleteCommand { get; }

    public bool HasTemplates
    {
        get => _hasTemplates;
        set => SetProperty(ref _hasTemplates, value);
    }

    public async Task RefreshAsync()
    {
        TemplateFileNames.Clear();
        var templates = await _templateLoader.GetAllTemplatesAsync();
        foreach (var template in templates)
        {
            TemplateFileNames.Add(template);
        }
        HasTemplates = TemplateFileNames.Count > 0;
    }

    private void Delete(string template)
    {
        _templateLoader.Delete(template);
    }

    public async Task<Script> LoadAsync(string templateName)
    {
        return await _templateLoader.LoadAsync(templateName);
    }

    public Task SaveAsync(string templateName, Script script)
    {
        return _templateLoader.SaveAsync(templateName, script);
    }
}