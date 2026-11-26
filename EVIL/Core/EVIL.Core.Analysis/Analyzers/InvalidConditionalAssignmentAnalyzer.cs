namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;

public sealed class InvalidConditionalAssignmentAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.InvalidConditionalAssignment
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<AssignmentExpression>(AnalyzeAssignmentExpression);
    }

    private static void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext<AssignmentExpression> context)
    {
        var node = context.Node;

        if (node.Left is not IndexerExpression ie)
        {
            return;
        }

        if (!ie.IsConditional && HasInnerConditionalIndexer(ie))
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.InvalidConditionalAssignment.Create(
                    context.GetLocation(node.Left),
                    ie.BuildChainStringRepresentation()
                )
            );
            return;
        }

        var root = GetChainRoot(ie);
        
        if (ie.IsConditional && (root is TableExpression or ArrayExpression or ConstantExpression))
        {
            context.ReportDiagnostic(
                CompilerDiagnostics.InvalidConditionalAssignment.Create(
                    context.GetLocation(node.Left),
                    ie.BuildChainStringRepresentation()
                )
            );
        }
    }

    private static bool HasInnerConditionalIndexer(IndexerExpression ie)
    {
        var current = ie.Indexable;
        
        while (current is IndexerExpression inner)
        {
            if (inner.IsConditional)
            {
                return true;
            }

            current = inner.Indexable;
        }

        return false;
    }

    private static Expression GetChainRoot(IndexerExpression ie)
    {
        var current = ie.Indexable;
        
        while (current is IndexerExpression inner)
        {
            current = inner.Indexable;
        }

        return current;
    }
}
