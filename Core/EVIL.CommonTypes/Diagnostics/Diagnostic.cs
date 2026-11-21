namespace EVIL.CommonTypes.Diagnostics;

using System.Collections.Generic;
using System.Text;

public sealed record Diagnostic(
    DiagnosticDescriptor Descriptor,
    SourceLocation Location,
    DiagnosticSeverity Severity,
    string Message,
    IReadOnlyList<object>? Arguments = null,
    IReadOnlyList<RelatedLocation>? RelatedLocations = null,
    IReadOnlyList<CodeFix>? SuggestedFixes = null
)
{
    public string Id => Descriptor.Id;

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Location.HasLocation)
        {
            sb.Append(Location.ToString());
            sb.Append(": ");
        }

        sb.Append(Severity.ToString().ToLower());
        sb.Append(' ');
        sb.Append(Id);
        sb.Append(": ");
        sb.Append(Message);

        if (RelatedLocations is { Count: > 0 })
        {
            foreach (var related in RelatedLocations)
            {
                sb.AppendLine();
                sb.Append("  -> ");
                if (related.Location.HasLocation)
                {
                    sb.Append(related.Location.ToString());
                    sb.Append(": ");
                }
                sb.Append(related.Message);
            }
        }

        return sb.ToString();
    }
}
