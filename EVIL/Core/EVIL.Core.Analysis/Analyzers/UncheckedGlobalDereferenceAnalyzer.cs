namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public sealed class UncheckedGlobalDereferenceAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.UncheckedGlobalDereference
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<IndexerExpression>(AnalyzeIndexerExpression);
    }

    private static void AnalyzeIndexerExpression(SyntaxNodeAnalysisContext<IndexerExpression> context)
    {
        var node = context.Node;

        if (!node.IsConditional && node.Indexable is SymbolReferenceExpression sym)
        {
            if (context.SymbolResolver.IsDynamicAmbientGlobal(sym.Identifier, node))
            {
                context.ReportDiagnostic(
                    CompilerDiagnostics.UncheckedGlobalDereference.Create(
                        context.GetLocation(node),
                        sym.Identifier,
                        node.GetKeyStringRepresentation()
                    )
                );
            }
        }
    }
}
