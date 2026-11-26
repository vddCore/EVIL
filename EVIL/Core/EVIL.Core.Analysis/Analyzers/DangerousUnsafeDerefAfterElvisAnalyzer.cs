namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public sealed class DangerousUnsafeDerefAfterElvisAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.DangerousUnsafeDerefAfterElvis
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<IndexerExpression>(AnalyzeIndexerExpression);
    }

    private static void AnalyzeIndexerExpression(SyntaxNodeAnalysisContext<IndexerExpression> context)
    {
        var node = context.Node;
        if (node.IsConditional)
        {
            return;
        }

        if (node.Indexable is IndexerExpression { IsConditional: true })
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.DangerousUnsafeDerefAfterElvis.Create(
                    context.GetLocation(node),
                    node.GetKeyStringRepresentation()
                )
            );
        }
    }
}
