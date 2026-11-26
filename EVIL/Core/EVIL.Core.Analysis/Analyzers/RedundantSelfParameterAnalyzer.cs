namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public sealed class RedundantSelfParameterAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.RedundantSelfParameter
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<FnTargetedStatement>(AnalyzeFnTargeted);
    }

    private static void AnalyzeFnTargeted(SyntaxNodeAnalysisContext<FnTargetedStatement> context)
    {
        if (context.Node.ParameterList is { HasSelf: true } parameterList)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.RedundantSelfParameter.Create(
                    context.GetLocation(parameterList)
                )
            );
        }
    }
}
