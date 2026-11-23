namespace EVIL.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public sealed class UncheckedNilInvocationAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.UncheckedNilInvocation
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<InvocationExpression>(AnalyzeInvocationExpression);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext<InvocationExpression> context)
    {
        var node = context.Node;

        if (node.Callee is IndexerExpression { IsConditional: true } ie)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.UncheckedNilInvocation.Create(
                    context.GetLocation(node),
                    ie.GetKeyStringRepresentation()
                )
            );
        }
    }
}
