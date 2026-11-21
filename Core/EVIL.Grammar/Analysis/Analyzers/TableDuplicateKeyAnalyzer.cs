namespace EVIL.Grammar.Analysis.Analyzers;

using System.Collections.Generic;
using System.Globalization;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

public sealed class TableDuplicateKeyAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [CompilerDiagnostics.TableDuplicateKey];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction<TableExpression>(AnalyzeTableExpression);
    }

    private static void AnalyzeTableExpression(SyntaxNodeAnalysisContext<TableExpression> context)
    {
        if (!context.Node.Keyed) return;

        var seenKeys = new HashSet<string>();

        foreach (var init in context.Node.Initializers)
        {
            if (init is KeyValuePairExpression kvp && TryGetKeyName(kvp.KeyNode, out var keyName))
            {
                if (!seenKeys.Add(keyName))
                {
                    context.ReportDiagnostic(
                        CompilerDiagnostics.TableDuplicateKey.Create(
                            context.GetLocation(kvp.KeyNode),
                            keyName
                        )
                    );
                }
            }
        }
    }

    private static bool TryGetKeyName(AST.Base.Expression keyNode, out string keyName)
    {
        switch (keyNode)
        {
            case StringConstant sc:
                keyName = sc.Value;
                return true;
            
            case NumberConstant nc:
                keyName = nc.Value.ToString(CultureInfo.InvariantCulture);
                return true;
            
            case BooleanConstant bc:
                keyName = bc.Value ? "true" : "false";
                return true;
            
            default:
                keyName = string.Empty;
                return false;
        }
    }
}
