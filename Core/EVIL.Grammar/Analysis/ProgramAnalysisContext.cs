namespace EVIL.Grammar.Analysis;

using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public readonly struct ProgramAnalysisContext
{
    public ProgramNode Program { get; }
    public string FilePath { get; }
    public IDiagnosticSink DiagnosticSink { get; }

    public ProgramAnalysisContext(ProgramNode program, string filePath, IDiagnosticSink diagnosticSink)
    {
        Program = program;
        FilePath = filePath;
        DiagnosticSink = diagnosticSink;
    }

    public void ReportDiagnostic(Diagnostic diagnostic) 
        => DiagnosticSink.Report(diagnostic);
    
    public SourceLocation GetLocation(AstNode node)
        => new(FilePath, node.Line, node.Column);
}
