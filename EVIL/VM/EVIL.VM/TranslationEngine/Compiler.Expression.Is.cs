namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
            Chunk.CodeGenerator.Emit(OpCode.LDTYPE, (byte)isExpression.Type.Value);
        }
        
        Chunk.CodeGenerator.Emit(isExpression.Invert ? OpCode.CNE : OpCode.CEQ);
    }
}