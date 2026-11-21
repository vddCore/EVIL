namespace EVIL.Ceres.TranslationEngine.Diagnostics;

using System.Text;
using EVIL.CommonTypes.Diagnostics;

public sealed record CompilerMessage(
    CompilerMessageSeverity Severity,
    string Message,
    string? FileName = null,
    int MessageCode = 0,
    int Line = 0,
    int Column = 0
)
{
    public SourceLocation Location => new(FileName, Line, Column);

    public Diagnostic ToDiagnostic()
    {
        var diagSeverity = Severity switch
        {
            CompilerMessageSeverity.Verbose => DiagnosticSeverity.Info,
            CompilerMessageSeverity.Warning => DiagnosticSeverity.Warning,
            CompilerMessageSeverity.Fatal => DiagnosticSeverity.Fatal,
            CompilerMessageSeverity.InternalFailure => DiagnosticSeverity.InternalError,
            _ => DiagnosticSeverity.Error
        };

        var descriptor = new DiagnosticDescriptor(
            MessageCode > 0 ? $"EV{MessageCode:D4}" : "EV0000",
            Severity.ToString(),
            Message,
            DiagnosticCategory.Codegen,
            diagSeverity
        );

        return new Diagnostic(descriptor, Location, diagSeverity, Message);
    }

    public static CompilerMessage FromDiagnostic(Diagnostic diagnostic)
    {
        var severity = diagnostic.Severity switch
        {
            DiagnosticSeverity.Hidden or DiagnosticSeverity.Info => CompilerMessageSeverity.Verbose,
            DiagnosticSeverity.Warning => CompilerMessageSeverity.Warning,
            DiagnosticSeverity.Error or DiagnosticSeverity.Fatal => CompilerMessageSeverity.Fatal,
            DiagnosticSeverity.InternalError => CompilerMessageSeverity.InternalFailure,
            _ => CompilerMessageSeverity.Fatal
        };

        var code = 0;
        if (diagnostic.Id.StartsWith("EV") && int.TryParse(diagnostic.Id.Substring(2), out var parsedCode))
        {
            code = parsedCode;
        }

        return new CompilerMessage(
            severity,
            diagnostic.Message,
            diagnostic.Location.FilePath,
            code,
            diagnostic.Location.Line,
            diagnostic.Location.Column
        );
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (MessageCode > 0)
        {
            sb.Append($"EV{MessageCode:D4}");
        }

        sb.Append($" :: {Severity}");
        if (Line > 0)
        {
            sb.Append($" :: line {Line}");

            if (Column > 0)
            {
                sb.Append($", column {Column}");
            }
        }
            
        if (!string.IsNullOrEmpty(FileName))
        {
            sb.Append($" :: {FileName}");
        }
            
        sb.Append($" :: {Message}");
        return sb.ToString();
    }
}
