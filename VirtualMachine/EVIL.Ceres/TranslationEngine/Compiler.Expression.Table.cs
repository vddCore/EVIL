namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(TableExpression tableExpression)
    {
        if (tableExpression.Initializers.Count == 0)
        {
            Chunk.CodeGenerator.Emit(OpCode.TABNEW);
            return;
        }

        if (tableExpression.Keyed)
        {
            foreach (var expr in tableExpression.Initializers)
            {
                var kvpe = (KeyValuePairExpression)expr;
                Visit(kvpe.ValueNode);
                Visit(kvpe.KeyNode);
            }

            Chunk.CodeGenerator.Emit(
                OpCode.TABINIT,
                -tableExpression.Initializers.Count
            );
        }
        else
        {
            for (var i = 0; i < tableExpression.Initializers.Count; i++)
            {
                Visit(tableExpression.Initializers[i]);
            }

            Chunk.CodeGenerator.Emit(
                OpCode.TABINIT,
                tableExpression.Initializers.Count
            );
        }
    }
}