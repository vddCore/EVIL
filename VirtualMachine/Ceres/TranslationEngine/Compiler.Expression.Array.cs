using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ArrayExpression arrayExpression)
        {
            int? knownSizeConstraint = null;
            
            if (arrayExpression.SizeExpression == null)
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDNUM,
                    (double)arrayExpression.Initializers.Count
                );
            }
            else
            {
                if (arrayExpression.SizeExpression.Reduce() is NumberConstant nc)
                {
                    knownSizeConstraint = (int)nc.Value;
                }
                else
                {
                    Visit(arrayExpression.SizeExpression);
                }
            }

            if (knownSizeConstraint != null)
            {
                if (arrayExpression.Initializers.Count > knownSizeConstraint.Value)
                {
                    Log.TerminateWithFatal(
                        "Attempt to initialize a constant size array with too many entries.",
                        CurrentFileName,
                        EvilMessageCode.TooManyInitializersForConstSizeArray,
                        arrayExpression.Line,
                        arrayExpression.Column
                    );
                }
                
                Chunk.CodeGenerator.Emit(
                    OpCode.LDNUM,
                    (double)knownSizeConstraint.Value
                );
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