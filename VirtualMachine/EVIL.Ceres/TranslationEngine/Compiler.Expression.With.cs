namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(WithExpression withExpression)
    {
        Visit(withExpression.BaseExpression);
        Chunk.CodeGenerator.Emit(OpCode.TABCLN, /* isDeepCopy: */ false);

        if (!withExpression.TableExpansionExpression.Keyed)
        {
            Log.TerminateWithFatal(
                "Expected a keyed expansion table.",
                CurrentFileName,
                EvilMessageCode.KeyedExpansionTableExpected,
                withExpression.TableExpansionExpression.Line,
                withExpression.TableExpansionExpression.Column
            );

            return;
        }

        var initializers = withExpression.TableExpansionExpression.Initializers;
        for (var i = 0; i < initializers.Count; i++)
        {
            var kvp = (KeyValuePairExpression)initializers[i];

            Visit(kvp.ValueNode);
            Visit(kvp.KeyNode);
            Chunk.CodeGenerator.Emit(OpCode.ELINIT);
        }
    }
}