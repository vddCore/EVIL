namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;

public sealed class RedundantNilCoalesceAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.RedundantNilCoalesce
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<CoalescingExpression>(AnalyzeCoalescingExpression);
    }

    private static void AnalyzeCoalescingExpression(SyntaxNodeAnalysisContext<CoalescingExpression> context)
    {
        var node = context.Node;

        if (node.Right is NilConstant)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.RedundantNilCoalesce.Create(
                    context.GetLocation(node),
                    "nil"
                )
            );
            return;
        }

        if (context.SymbolResolver.IsGuaranteedSafe(node.Left))
        {
            var leftName = node.Left switch
            {
                SymbolReferenceExpression sym => sym.Identifier,
                SelfExpression => "self",
                TableExpression => "{ ... }",
                ArrayExpression => "[ ... ]",
                _ => node.Left.ToString() ?? "expression"
            };

            context.ReportDiagnostic(
                CompilerDiagnostics.RedundantNilCoalesce.Create(
                    context.GetLocation(node),
                    leftName
                )
            );
        }
    }
}
