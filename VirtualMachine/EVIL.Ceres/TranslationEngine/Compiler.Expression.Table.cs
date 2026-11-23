namespace EVIL.Ceres.TranslationEngine;

using System.Linq;
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

        var hasSpread = tableExpression.Initializers.Any(i => i is TableSpreadExpression);

        if (tableExpression.Keyed)
        {
            if (!hasSpread)
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
                Chunk.CodeGenerator.Emit(OpCode.TABNEW);

                foreach (var expr in tableExpression.Initializers)
                {
                    if (expr is TableSpreadExpression spread)
                    {
                        Visit(spread);
                    }
                    else if (expr is KeyValuePairExpression kvpe)
                    {
                        Visit(kvpe.ValueNode);
                        Visit(kvpe.KeyNode);
                        Chunk.CodeGenerator.Emit(OpCode.ELINIT);
                    }
                }
            }
        }
        else
        {
            if (!hasSpread)
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
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.TABNEW);
                var arrayIndex = 0;

                foreach (var expr in tableExpression.Initializers)
                {
                    if (expr is TableSpreadExpression spread)
                    {
                        Visit(spread);
                    }
                    else
                    {
                        Visit(expr);
                        Chunk.CodeGenerator.Emit(OpCode.LDNUM, (double)arrayIndex++);
                        Chunk.CodeGenerator.Emit(OpCode.ELINIT);
                    }
                }
            }
        }
    }
}