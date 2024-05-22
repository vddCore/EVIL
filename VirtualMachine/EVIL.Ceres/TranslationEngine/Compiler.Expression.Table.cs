using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(TableExpression tableExpression)
        {
            Chunk.CodeGenerator.Emit(OpCode.TABNEW);

            if (tableExpression.Keyed)
            {
                foreach (var expr in tableExpression.Initializers)
                {
                    var kvpe = (KeyValuePairExpression)expr;
                    Visit(kvpe.ValueNode);
                    Visit(kvpe.KeyNode);
                    Chunk.CodeGenerator.Emit(OpCode.ELINIT);
                }
            }
            else
            {
                for (var i = 0; i < tableExpression.Initializers.Count; i++)
                {
                    Visit(tableExpression.Initializers[i]);

                    if (i == 0)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.LDZERO);
                    }
                    else if (i == 1)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.LDONE);
                    }
                    else
                    {
                        Chunk.CodeGenerator.Emit(OpCode.LDNUM, (double)i);
                    }

                    Chunk.CodeGenerator.Emit(OpCode.ELINIT);
                }
            }
        }
    }
}