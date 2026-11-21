namespace EVIL.Grammar.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;

public sealed class UnreachableCodeAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [CompilerDiagnostics.UnreachableCode];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<BlockStatement>(AnalyzeBlockStatement);
    }

    private static void AnalyzeBlockStatement(SyntaxNodeAnalysisContext<BlockStatement> context)
    {
        var statements = context.Node.Statements;
        
        for (var i = 0; i < statements.Count - 1; i++)
        {
            var stmt = statements[i];
            
            if (TryGetTerminatorName(stmt, out var terminatorName))
            {
                var nextStmt = statements[i + 1];
                context.ReportDiagnostic(
                    CompilerDiagnostics.UnreachableCode.Create(
                        context.GetLocation(nextStmt),
                        terminatorName
                    )
                );

                // no need to report it twice.
                break;
            }
        }
    }

    private static bool TryGetTerminatorName(Statement statement, out string terminatorName)
    {
        switch (statement)
        {
            case RetStatement:
                terminatorName = "ret";
                return true;
            
            case ThrowStatement:
                terminatorName = "throw";
                return true;
            
            case BreakStatement:
                terminatorName = "break";
                return true;
            
            case SkipStatement:
                terminatorName = "skip";
                return true;
            
            case RetryStatement:
                terminatorName = "retry";
                return true;
            
            default:
                terminatorName = string.Empty;
                return false;
        }
    }
}
