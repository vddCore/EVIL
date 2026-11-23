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
        context.RegisterSyntaxNodeAction<SelfInvocationExpression>(AnalyzeSelfInvocationExpression);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext<InvocationExpression> context)
    {
        var node = context.Node;

        if (!node.IsConditional
            && node.Callee is IndexerExpression { IsConditional: true } ie)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.UncheckedNilInvocation.Create(
                    context.GetLocation(node),
                    ie.GetKeyStringRepresentation()
                )
            );
        }
    }

    private static void AnalyzeSelfInvocationExpression(SyntaxNodeAnalysisContext<SelfInvocationExpression> context)
    {
        var node = context.Node;

        if (!node.IsConditional 
            && node.Indexable is IndexerExpression { IsConditional: true })
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.UncheckedNilInvocation.Create(
                    context.GetLocation(node),
                    node.Identifier.Name
                )
            );
        }
    }
}
