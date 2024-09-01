﻿using CppPad.CompilerAdapter.Interface;

namespace CppPad.ScriptFile.Interface;

public record Script
{
    public int Version { get; init; } = 1;

    public string Content { get; init; } = string.Empty;

    public IReadOnlyList<string> AdditionalIncludeDirs { get; init; } = [];

    public CppStandard CppStandard { get; init; } = CppStandard.CppLatest;

    public OptimizationLevel OptimizationLevel { get; init; } = OptimizationLevel.Unspecified;

    public string AdditionalBuildArgs { get; init; } = string.Empty;
}