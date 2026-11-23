namespace EVIL.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

public sealed class NilBranchExhaustivenessAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.NilBranchExhaustiveness
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<ByExpression>(AnalyzeByExpression);
    }

    private static void AnalyzeByExpression(SyntaxNodeAnalysisContext<ByExpression> context)
    {
        var node = context.Node;

        if (node.ElseArm != null)
        {
            return;
        }

        if (node.Qualifier is not IndexerExpression { IsConditional: true } ie)
        {
            return;
        }

        var hasNilArm = false;
        foreach (var arm in node.Arms)
        {
            if (arm.Selector is NilConstant)
            {
                hasNilArm = true;
                break;
            }
        }

        if (!hasNilArm)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.NilBranchExhaustiveness.Create(
                    context.GetLocation(node),
                    ie.BuildChainStringRepresentation()
                )
            );
        }
    }
}
