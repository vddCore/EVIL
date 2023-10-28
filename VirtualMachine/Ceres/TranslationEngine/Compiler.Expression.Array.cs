using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ArrayExpression arrayExpression)
        {
            if (arrayExpression.SizeExpression == null)
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDNUM,
                    (double)arrayExpression.Initializers.Count
                );
            }
            else
            {
                Visit(arrayExpression.SizeExpression);
            }
            
            Chunk.CodeGenerator.Emit(OpCode.ARRNEW);
            if (arrayExpression.Initializers.Any())
            {
                for (var i = 0; i < arrayExpression.Initializers.Count; i++)
                {
                    Visit(arrayExpression.Initializers[i]);

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