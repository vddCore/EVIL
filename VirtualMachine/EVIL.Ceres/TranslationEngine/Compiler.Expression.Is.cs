namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(IsExpression isExpression)
    {
        Visit(isExpression.Target);

        if (isExpression.IsManagedTypeCheck)
        {
            Chunk.CodeGenerator.Emit(OpCode.NTYPE);
            Visit(isExpression.ManagedTypeName!);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
            Chunk.CodeGenerator.Emit(OpCode.LDTYPE, (int)isExpression.Type.Value);
        }
        
        Chunk.CodeGenerator.Emit(isExpression.Invert ? OpCode.CNE : OpCode.CEQ);
    }
}