namespace EVIL.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

public sealed class RedundantNullCoalesceAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.RedundantNullCoalesce
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
                CompilerDiagnostics.RedundantNullCoalesce.Create(
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
                CompilerDiagnostics.RedundantNullCoalesce.Create(
                    context.GetLocation(node),
                    leftName
                )
            );
        }
    }
}
