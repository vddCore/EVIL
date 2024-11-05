namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(TypeOfExpression typeOfExpression)
    {
        Visit(typeOfExpression.Target);

        if (typeOfExpression.IsNative)
        {
            Chunk.CodeGenerator.Emit(OpCode.NTYPE);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
        }
    }
}