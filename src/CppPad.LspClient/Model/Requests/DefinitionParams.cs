﻿using System.Text.Json.Serialization;

namespace CppPad.LspClient.Model.Requests;

public class DefinitionParams
{
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; init; } = new();

    [JsonPropertyName("position")]
    public Position Position { get; init; } = new();
}
