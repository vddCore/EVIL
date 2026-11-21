namespace EVIL.Grammar.Analysis;

using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Base;

public readonly struct SyntaxNodeAnalysisContext
{
    public AstNode Node { get; }
    public string FilePath { get; }
    public IDiagnosticSink DiagnosticSink { get; }

    public SyntaxNodeAnalysisContext(AstNode node, string filePath, IDiagnosticSink diagnosticSink)
    {
        Node = node;
        FilePath = filePath;
        DiagnosticSink = diagnosticSink;
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
    public TNode Node { get; }
    public string FilePath { get; }
    public IDiagnosticSink DiagnosticSink { get; }

    public SyntaxNodeAnalysisContext(TNode node, string filePath, IDiagnosticSink diagnosticSink)
    {
        Node = node;
        FilePath = filePath;
        DiagnosticSink = diagnosticSink;
    }

    public void ReportDiagnostic(Diagnostic diagnostic) 
        => DiagnosticSink.Report(diagnostic);

    public SourceLocation GetLocation() 
        => new(FilePath, Node.Line, Node.Column);

    public SourceLocation GetLocation(AstNode node) 
        => new(FilePath, node.Line, node.Column);
}
