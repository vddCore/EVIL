namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;

public sealed class NilComparisonTypeMismatchAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.NilComparisonTypeMismatch
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<BinaryExpression>(AnalyzeBinaryExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext<BinaryExpression> context)
    {
        var node = context.Node;

        if (node.Type is BinaryOperationType.Less
            or BinaryOperationType.Greater
            or BinaryOperationType.LessOrEqual
            or BinaryOperationType.GreaterOrEqual)
        {
            if (node.Left is NilConstant || node.Right is NilConstant)
            {
                var opStr = node.Type switch
                {
                    BinaryOperationType.Less => "<",
                    BinaryOperationType.Greater => ">",
                    BinaryOperationType.LessOrEqual => "<=",
                    BinaryOperationType.GreaterOrEqual => ">=",
                    _ => node.Type.ToString()
                };

                context.ReportDiagnostic(
                    CompilerDiagnostics.NilComparisonTypeMismatch.Create(
                        context.GetLocation(node),
                        opStr
                    )
                );
            }
        }
    }
}
