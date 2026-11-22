namespace EVIL.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Statements;

public sealed class CatchlessTryAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [CompilerDiagnostics.CatchlessTry];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<TryStatement>(AnalyzeTryStatement);
    }

    private static void AnalyzeTryStatement(SyntaxNodeAnalysisContext<TryStatement> context)
    {
        if (context.Node.HandlerStatement == null)
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.CatchlessTry.Create(context.GetLocation())
            );
        }
    }
}
