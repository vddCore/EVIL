namespace EVIL.Diagnostics.Formatters;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class RichDiagnosticFormatter : DiagnosticFormatter
{
    private const string Reset = "\u001b[0m";
    private const string Bold = "\u001b[1m";
    private const string RedBold = "\u001b[31;1m";
    private const string YellowBold = "\u001b[33;1m";
    private const string CyanBold = "\u001b[36;1m";
    private const string BlueBold = "\u001b[34;1m";

    public bool UseColor { get; set; } = true;
    public Func<string, string?>? SourceProvider { get; set; }

    public override string Format(Diagnostic diagnostic)
    {
        var sb = new StringBuilder();

        var (severityColor, severityLabel) = diagnostic.Severity switch
        {
            DiagnosticSeverity.Hidden => (CyanBold, "hidden"),
            DiagnosticSeverity.Info => (CyanBold, "info"),
            DiagnosticSeverity.Warning => (YellowBold, "warning"),
            DiagnosticSeverity.Error => (RedBold, "error"),
            DiagnosticSeverity.Fatal => (RedBold, "fatal error"),
            DiagnosticSeverity.InternalError => (RedBold, "internal error"),
            _ => (RedBold, "error")
        };

        var colorPrefix = UseColor ? severityColor : string.Empty;
        var boldPrefix = UseColor ? Bold : string.Empty;
        var bluePrefix = UseColor ? BlueBold : string.Empty;
        var reset = UseColor ? Reset : string.Empty;

        sb.Append(colorPrefix);
        sb.Append(severityLabel);
        sb.Append('[');
        sb.Append(diagnostic.Id);
        sb.Append(']');
        sb.Append(reset);
        sb.Append(": ");
        sb.Append(boldPrefix);
        sb.Append(diagnostic.Message);
        sb.Append(reset);

        if (diagnostic.Location.HasLocation)
        {
            var filePath = !string.IsNullOrEmpty(diagnostic.Location.FilePath) 
                ? diagnostic.Location.FilePath 
                : "<source>";
            var line = diagnostic.Location.Line;
            var col = diagnostic.Location.Column;

            sb.AppendLine();
            sb.Append("  ");
            sb.Append(bluePrefix);
            sb.Append("--> ");
            sb.Append(reset);
            sb.Append(filePath);
            sb.Append(':');
            sb.Append(line);
            sb.Append(':');
            sb.Append(col);

            var sourceLine = GetSourceLine(diagnostic.Location.FilePath, line);
            if (sourceLine != null)
            {
                var gutterWidth = Math.Max(line.ToString().Length, 2);
                var emptyGutter = new string(' ', gutterWidth);

                sb.AppendLine();
                sb.Append("  ");
                sb.Append(bluePrefix);
                sb.Append(emptyGutter);
                sb.Append(" |");
                sb.Append(reset);

                sb.AppendLine();
                sb.Append("  ");
                sb.Append(bluePrefix);
                sb.Append(line.ToString().PadLeft(gutterWidth));
                sb.Append(" | ");
                sb.Append(reset);
                sb.Append(sourceLine);

                var caretOffset = Math.Max(0, col - 1);
                var caretLength = Math.Max(1, diagnostic.Location.Length);

                sb.AppendLine();
                sb.Append("  ");
                sb.Append(bluePrefix);
                sb.Append(emptyGutter);
                sb.Append(" | ");
                sb.Append(reset);
                sb.Append(new string(' ', caretOffset));
                sb.Append(colorPrefix);
                sb.Append(new string('^', caretLength));
                sb.Append(reset);

                sb.AppendLine();
                sb.Append("  ");
                sb.Append(bluePrefix);
                sb.Append(emptyGutter);
                sb.Append(" |");
                sb.Append(reset);
            }
        }

        if (diagnostic.RelatedLocations is { Count: > 0 })
        {
            foreach (var rel in diagnostic.RelatedLocations)
            {
                sb.AppendLine();
                sb.Append("  ");
                sb.Append(bluePrefix);
                sb.Append("::: ");
                sb.Append(reset);
                if (rel.Location.HasLocation)
                {
                    var relFile = !string.IsNullOrEmpty(rel.Location.FilePath)
                        ? rel.Location.FilePath
                        : "<source>";
                    sb.Append(relFile);
                    sb.Append(':');
                    sb.Append(rel.Location.Line);
                    sb.Append(':');
                    sb.Append(rel.Location.Column);
                    sb.Append(": ");
                }
                sb.Append(rel.Message);
            }
        }

        if (diagnostic.SuggestedFixes is { Count: > 0 })
        {
            foreach (var fix in diagnostic.SuggestedFixes)
            {
                sb.AppendLine();
                sb.Append("  ");
                sb.Append(cyanPrefix(UseColor));
                sb.Append("help: ");
                sb.Append(reset);
                sb.Append(fix.Title);
            }
        }

        return sb.ToString();
    }

    private static string cyanPrefix(bool useColor) => useColor ? CyanBold : string.Empty;

    public override string FormatAll(IEnumerable<Diagnostic> diagnostics)
    {
        return string.Join(Environment.NewLine + Environment.NewLine, diagnostics.Select(Format));
    }

    private string? GetSourceLine(string? filePath, int lineNumber)
    {
        if (lineNumber <= 0)
            return null;

        string? content = null;

        if (SourceProvider != null && !string.IsNullOrEmpty(filePath))
        {
            content = SourceProvider(filePath);
        }

        if (content == null && SourceProvider != null)
        {
            content = SourceProvider(string.Empty);
        }

        if (content == null && !string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            try
            {
                content = File.ReadAllText(filePath);
            }
            catch
            {
                return null;
            }
        }

        if (string.IsNullOrEmpty(content))
            return null;

        var lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        if (lineNumber <= lines.Length)
        {
            return lines[lineNumber - 1];
        }

        return null;
    }
}
