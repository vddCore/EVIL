namespace EVIL.Core.Analysis;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Base;

public readonly struct SyntaxNodeAnalysisContext
{
    public string FilePath { get; }
    
    public AstNode Node { get; }
    public IDiagnosticSink DiagnosticSink { get; }
    public GlobalSymbolResolver SymbolResolver { get; }

    public SyntaxNodeAnalysisContext(
        string filePath,
        AstNode node,
        IDiagnosticSink diagnosticSink,
        GlobalSymbolResolver? symbolResolver = null)
    {
        FilePath = filePath;
        
        Node = node;
        DiagnosticSink = diagnosticSink;
        SymbolResolver = symbolResolver ?? GlobalSymbolResolver.Default;
    }

    public void ReportDiagnostic(Diagnostic diagnostic)
        => DiagnosticSink.Report(diagnostic);

    public SourceLocation GetLocation() 
        => new(FilePath, Node.Line, Node.Column);

    public SourceLocation GetLocation(AstNode node) 
        => new(FilePath, node.Line, node.Column);
}

public readonly struct SyntaxNodeAnalysisContext<TNode> where TNode : AstNode
{
    public string FilePath { get; }
    
    public TNode Node { get; }
    public IDiagnosticSink DiagnosticSink { get; }
    public GlobalSymbolResolver SymbolResolver { get; }

    public SyntaxNodeAnalysisContext(
        string filePath,
        TNode node,
        IDiagnosticSink diagnosticSink,
        GlobalSymbolResolver? symbolResolver = null)
    {
        FilePath = filePath;
        
        Node = node;
        DiagnosticSink = diagnosticSink;
        SymbolResolver = symbolResolver ?? GlobalSymbolResolver.Default;
    }

    public void ReportDiagnostic(Diagnostic diagnostic) 
        => DiagnosticSink.Report(diagnostic);

    public SourceLocation GetLocation() 
        => new(FilePath, Node.Line, Node.Column);

    public SourceLocation GetLocation(AstNode node) 
        => new(FilePath, node.Line, node.Column);
}
