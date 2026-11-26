namespace EVIL.Core.Analysis;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public readonly struct ProgramAnalysisContext
{
    public string FilePath { get; }
    
    public ProgramNode Program { get; }
    public IDiagnosticSink DiagnosticSink { get; }
    public GlobalSymbolResolver SymbolResolver { get; }

    public ProgramAnalysisContext(string filePath,
        ProgramNode program,
        IDiagnosticSink diagnosticSink,
        GlobalSymbolResolver? symbolResolver = null)
    {
        FilePath = filePath;
        
        Program = program;
        DiagnosticSink = diagnosticSink;
        SymbolResolver = symbolResolver ?? GlobalSymbolResolver.Default;
    }

    public void ReportDiagnostic(Diagnostic diagnostic) 
        => DiagnosticSink.Report(diagnostic);
    
    public SourceLocation GetLocation(AstNode node)
        => new(FilePath, node.Line, node.Column);
}
