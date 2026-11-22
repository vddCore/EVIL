namespace EVIL.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public sealed class MissingByElseArmAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [CompilerDiagnostics.NoDefaultByArm];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<ByExpression>(AnalyzeByExpression);
    }

    private static void AnalyzeByExpression(SyntaxNodeAnalysisContext<ByExpression> context)
    {
        if (context.Node.ElseArm == null)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.NoDefaultByArm.Create(
                    context.GetLocation()
                )
            );
        }
    }
}
