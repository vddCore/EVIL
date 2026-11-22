namespace EVIL.Diagnostics.Formatters;

using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DiagnosticFormatter
{
    public static DiagnosticFormatter Standard { get; } = new StandardDiagnosticFormatter();
    public static DiagnosticFormatter Rich { get; } = new RichDiagnosticFormatter();
    public static DiagnosticFormatter Json { get; } = new JsonDiagnosticFormatter();

    public abstract string Format(Diagnostic diagnostic);

    public virtual string FormatAll(IEnumerable<Diagnostic> diagnostics)
    {
        return string.Join(Environment.NewLine, diagnostics.Select(Format));
    }

    public static DiagnosticFormatter GetByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Standard;

        return name.Trim().ToLowerInvariant() switch
        {
            "standard" => Standard,
            "rich" => Rich,
            "json" => Json,
            _ => throw new ArgumentException($"Unknown diagnostic format '{name}'. Supported formats: standard, rich, json.")
        };
    }
}
