namespace EVIL.VM.TranslationEngine;

using System;
using System.Collections.Generic;

public record CompilerOptions
{
    public static CompilerOptions Default => new()
    {
        OptimizeCodeGeneration = true,
        EnableAnalyzers = true,
        MaxErrors = 64,
        WarningsAsErrors = false,
        WarningsAsErrorsCodes = new(StringComparer.OrdinalIgnoreCase),
        SuppressedCodes = new(StringComparer.OrdinalIgnoreCase)
    };

    public bool OptimizeCodeGeneration { get; set; } = true;
    public bool EnableAnalyzers { get; set; } = true;
    public int MaxErrors { get; set; } = 64;

    public bool WarningsAsErrors { get; set; }
    public HashSet<string> WarningsAsErrorsCodes { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> SuppressedCodes { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}