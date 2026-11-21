namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(ArrayExpression arrayExpression)
    {
        int? knownSizeConstraint = null;
            
        if (arrayExpression.SizeExpression == null)
        {
            for (var i = 0; i < arrayExpression.Initializers.Count; i++)
            {
                Visit(arrayExpression.Initializers[i]);
            }

            Chunk.CodeGenerator.Emit(
                OpCode.ARRINIT,
                arrayExpression.Initializers.Count
            );
            
            return;
        }

        if (arrayExpression.SizeExpression.Reduce() is NumberConstant nc)
        {
            knownSizeConstraint = (int)nc.Value;
        }
        else
        {
            Visit(arrayExpression.SizeExpression);
        }

        if (knownSizeConstraint != null)
        {
            if (arrayExpression.Initializers.Count > knownSizeConstraint.Value)
            {
                Log.TerminateWithFatal(
                    CompilerDiagnostics.TooManyInitializersForConstSizeArray.Create(
                        GetLocation(arrayExpression)
                    )
                );
            }
                
            Chunk.CodeGenerator.Emit(
                OpCode.LDNUM,
                (double)knownSizeConstraint.Value
            );
        }
            
        Chunk.CodeGenerator.Emit(OpCode.ARRNEW);
        if (arrayExpression.Initializers.Count != 0)
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