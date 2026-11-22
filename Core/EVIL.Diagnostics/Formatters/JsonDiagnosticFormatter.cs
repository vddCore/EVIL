namespace EVIL.Diagnostics.Formatters;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonDiagnosticFormatter : DiagnosticFormatter
{
    public bool Indented { get; set; } = true;

    private JsonSerializerOptions GetOptions() => new()
    {
        WriteIndented = Indented,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public override string Format(Diagnostic diagnostic)
    {
        var model = ToModel(diagnostic);
        return JsonSerializer.Serialize(model, GetOptions());
    }

    public override string FormatAll(IEnumerable<Diagnostic> diagnostics)
    {
        var models = diagnostics.Select(ToModel).ToList();
        return JsonSerializer.Serialize(models, GetOptions());
    }

    private static DiagnosticJsonModel ToModel(Diagnostic d)
    {
        LocationJsonModel? loc = null;
        if (d.Location.HasLocation)
        {
            loc = new LocationJsonModel(
                d.Location.FilePath,
                d.Location.Line,
                d.Location.Column,
                d.Location.EndLine,
                d.Location.EndColumn,
                d.Location.Offset,
                d.Location.Length
            );
        }

        var related = d.RelatedLocations?.Select(r => new RelatedLocationJsonModel(
            r.Location.FilePath,
            r.Location.Line,
            r.Location.Column,
            r.Message
        )).ToList() ?? [];

        var fixes = d.SuggestedFixes?.Select(f => new CodeFixJsonModel(
            f.Title,
            f.ReplacementText
        )).ToList() ?? [];

        return new DiagnosticJsonModel(
            d.Id,
            d.Descriptor.Title,
            d.Severity.ToString(),
            (int)d.Severity,
            d.Descriptor.Category.ToString(),
            d.Message,
            loc,
            related,
            fixes
        );
    }

    private sealed record DiagnosticJsonModel(
        string Id,
        string Title,
        string Severity,
        int SeverityLevel,
        string Category,
        string Message,
        LocationJsonModel? Location,
        IReadOnlyList<RelatedLocationJsonModel> RelatedLocations,
        IReadOnlyList<CodeFixJsonModel> CodeFixes
    );

    private sealed record LocationJsonModel(
        string? FilePath,
        int Line,
        int Column,
        int EndLine,
        int EndColumn,
        int Offset,
        int Length
    );

    private sealed record RelatedLocationJsonModel(
        string? FilePath,
        int Line,
        int Column,
        string Message
    );

    private sealed record CodeFixJsonModel(
        string Description,
        string Replacement
    );
}
