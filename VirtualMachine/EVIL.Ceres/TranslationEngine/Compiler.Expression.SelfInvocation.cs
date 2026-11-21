namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SelfInvocationExpression selfInvocationExpression)
    {
        var tempLocal = Chunk.AllocateLocal();

        Visit(selfInvocationExpression.Indexable);
        Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempLocal);

        // Push 'self' as the first argument
        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);

        // Push subsequent arguments
        Visit(selfInvocationExpression.ArgumentList);

        // Push receiver for method indexing
        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);
            
        var identifierNameId = Chunk.StringPool.FetchOrAdd(
            selfInvocationExpression.Identifier.Name
        );
            
        Chunk.CodeGenerator.Emit(OpCode.LDSTR, (int)identifierNameId);
        Chunk.CodeGenerator.Emit(OpCode.INDEX);
            
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