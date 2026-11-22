namespace EVIL.Ceres.TranslationEngine;

using System;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(YieldExpression yieldExpression)
    {
        switch (yieldExpression.Invocation)
        {
            case InvocationExpression invocationExpression:
            {
                Visit(invocationExpression.ArgumentList);
                Visit(invocationExpression.Callee);

                Chunk.CodeGenerator.Emit(
                    OpCode.YIELD,
                    invocationExpression.ArgumentList.Arguments.Count
                );

                Chunk.CodeGenerator.Emit(OpCode.YRET);
                break;
            }

            case SelfInvocationExpression selfInvocationExpression:
            {
                var tempLocal = Chunk.AllocateLocal();

                Visit(selfInvocationExpression.Indexable);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempLocal);
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);

                Visit(selfInvocationExpression.ArgumentList);
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempLocal);
            
                var identifierNameId = Chunk.StringPool.FetchOrAdd(
                    selfInvocationExpression.Identifier.Name
                );
            
                Chunk.CodeGenerator.Emit(OpCode.LDSTR, (int)identifierNameId);
                Chunk.CodeGenerator.Emit(OpCode.INDEX);
            
                Chunk.CodeGenerator.Emit(
                    OpCode.YIELD,
                    selfInvocationExpression.ArgumentList.Arguments.Count + 1 /* Implicit `self' */
                );

                Chunk.CodeGenerator.Emit(OpCode.YRET);
                break;
            }

            default:
                throw new InvalidOperationException(
                    $"Unsupported yield invocation type: {yieldExpression.Invocation.GetType().FullName}"
                );
        }
    }
}