namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

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