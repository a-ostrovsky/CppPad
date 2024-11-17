#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CppPad.AutoCompletion.Interface;
using CppPad.FileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace CppPad.Gui.ViewModels;

public class DefinitionDataViewModel
{
    public required string Label { get; set; }

    public required string SourceCode { get; set; }

    /// <summary>
    ///     Gets or sets the one-based line number.
    /// </summary>
    public required int Line { get; set; }

    /// <summary>
    ///     Gets or sets the zero-based (!) character index in the line.
    /// </summary>
    public required int Character { get; set; }

    public override string ToString()
    {
        return Label;
    }
}

public class DefinitionsViewModel(DiskFileSystem fileSystem, ILoggerFactory loggerFactory)
    : ViewModelBase
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<DefinitionsViewModel>();
    private DefinitionDataViewModel? _selectedDefinition;

    public static DefinitionsViewModel DesignInstance { get; } =
        new(new DiskFileSystem(), new NullLoggerFactory());

    public ObservableCollection<DefinitionDataViewModel> Definitions { get; } = [];

    public DefinitionDataViewModel? SelectedDefinition
    {
        get => _selectedDefinition;
        set => SetProperty(ref _selectedDefinition, value);
    }

    public async Task SetDefinitionsAsync(IEnumerable<PositionInFile> definitions)
    {
        Definitions.Clear();
        foreach (var def in definitions)
        {
            string sourceCode;
            try
            {
                sourceCode = await fileSystem.ReadAllTextAsync(def.FileName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read file {FileName} in order to load definition.",
                    def.FileName);
                continue;
            }

            var definition = new DefinitionDataViewModel
            {
                Label = $"{def.FileName}:{def.Position.Line + 1}:{def.Position.Character + 1}",
                SourceCode = sourceCode,
                Line = def.Position.Line + 1,
                Character = def.Position.Character
            };

            Definitions.Add(definition);
            SelectedDefinition = Definitions.FirstOrDefault();
        }
    }
}