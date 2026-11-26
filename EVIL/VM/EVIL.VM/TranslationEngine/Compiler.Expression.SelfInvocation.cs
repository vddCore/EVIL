namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SelfInvocationExpression selfInvocationExpression)
    {
        var tempLocal = Chunk.AllocateLocal();

        if (selfInvocationExpression.IsConditional)
        {
            var dispatchLabel = Chunk.CreateLabel();
            var endLabel = Chunk.CreateLabel();

            Visit(selfInvocationExpression.Indexable);
            Chunk.CodeGenerator.Emit(OpCode.DUP);
            Chunk.CodeGenerator.Emit(OpCode.VJMP, dispatchLabel);
            // If receiver is nil: DUP left nil on stack. Jump directly to end!
            Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);

            Chunk.UpdateLabel(dispatchLabel, Chunk.CodeGenerator.IP);
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempLocal);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);
            Visit(selfInvocationExpression.ArgumentList);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);
            
            var identifierNameId = Chunk.StringPool.FetchOrAdd(
                selfInvocationExpression.Identifier.Name
            );
            
            Chunk.CodeGenerator.EmitGetProp((int)identifierNameId);
            
            Chunk.CodeGenerator.Emit(
                OpCode.INVOKE,
                selfInvocationExpression.ArgumentList.Arguments.Count + 1 /* Implicit `self' */
            );
            
            Chunk.CodeGenerator.Emit(
                selfInvocationExpression.ArgumentList.IsVariadic
                    ? (byte)1
                    : (byte)0
            );

            Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
        }
        else
        {
            Visit(selfInvocationExpression.Indexable);
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempLocal);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);
            Visit(selfInvocationExpression.ArgumentList);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);
            
            var identifierNameId = Chunk.StringPool.FetchOrAdd(
                selfInvocationExpression.Identifier.Name
            );
            
            Chunk.CodeGenerator.EmitGetProp((int)identifierNameId);
            
            Chunk.CodeGenerator.Emit(
                OpCode.INVOKE,
                selfInvocationExpression.ArgumentList.Arguments.Count + 1 /* Implicit `self' */
            );
            
            Chunk.CodeGenerator.Emit(
                selfInvocationExpression.ArgumentList.IsVariadic
                    ? (byte)1
                    : (byte)0
            );
        }
    }
}
