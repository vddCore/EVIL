namespace EVIL.Core.Diagnostics.Formatters;

using System.Text;

public class StandardDiagnosticFormatter : DiagnosticFormatter
{
    public override string Format(Diagnostic diagnostic)
    {
        var sb = new StringBuilder();

        if (diagnostic.Location.HasLocation)
        {
            if (!string.IsNullOrEmpty(diagnostic.Location.FilePath))
            {
                sb.Append(diagnostic.Location.FilePath);
                sb.Append('(');
                sb.Append(diagnostic.Location.Line);
                sb.Append(',');
                sb.Append(diagnostic.Location.Column);
                sb.Append("): ");
            }
            else
            {
                sb.Append('(');
                sb.Append(diagnostic.Location.Line);
                sb.Append(',');
                sb.Append(diagnostic.Location.Column);
                sb.Append("): ");
            }
        }

        var severityStr = diagnostic.Severity switch
        {
            DiagnosticSeverity.Hidden => "hidden",
            DiagnosticSeverity.Info => "info",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Error => "error",
            DiagnosticSeverity.Fatal => "fatal",
            DiagnosticSeverity.InternalError => "internal error",
            _ => "error"
        };

        sb.Append(severityStr);
        sb.Append(' ');
        sb.Append(diagnostic.Id);
        sb.Append(": ");
        sb.Append(diagnostic.Message);

        if (diagnostic.RelatedLocations is { Count: > 0 })
        {
            foreach (var rel in diagnostic.RelatedLocations)
            {
                sb.AppendLine();
                sb.Append("  ");
                if (rel.Location.HasLocation)
                {
                    if (!string.IsNullOrEmpty(rel.Location.FilePath))
                    {
                        sb.Append(rel.Location.FilePath);
                        sb.Append('(');
                        sb.Append(rel.Location.Line);
                        sb.Append(',');
                        sb.Append(rel.Location.Column);
                        sb.Append("): ");
                    }
                    else
                    {
                        sb.Append('(');
                        sb.Append(rel.Location.Line);
                        sb.Append(',');
                        sb.Append(rel.Location.Column);
                        sb.Append("): ");
                    }
                }
                sb.Append("info: ");
                sb.Append(rel.Message);
            }
        }

        return sb.ToString();
    }
}
