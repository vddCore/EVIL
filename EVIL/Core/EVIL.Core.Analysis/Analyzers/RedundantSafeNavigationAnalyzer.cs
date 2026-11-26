namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public sealed class RedundantSafeNavigationAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.RedundantSafeNavigation
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<IndexerExpression>(AnalyzeIndexerExpression);
    }

    private static void AnalyzeIndexerExpression(SyntaxNodeAnalysisContext<IndexerExpression> context)
    {
        var node = context.Node;
        
        if (!node.IsConditional)
        {
            return;
        }

        if (context.SymbolResolver.IsGuaranteedSafe(node.Indexable))
        {
            var targetName = node.Indexable switch
            {
                SymbolReferenceExpression sym => sym.Identifier,
                SelfExpression => "self",
                TableExpression => "{ ... }",
                ArrayExpression => "[ ... ]",
                _ => node.Indexable.ToString() ?? "expression"
            };

            context.ReportDiagnostic(
                CompilerDiagnostics.RedundantSafeNavigation.Create(
                    context.GetLocation(node),
                    targetName
                )
            );
        }
    }
}
