namespace EVIL.VM.TranslationEngine;

using System;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
            
                Chunk.CodeGenerator.EmitGetProp((int)identifierNameId);
            
                Chunk.CodeGenerator.Emit(
                    OpCode.YIELD,
                    selfInvocationExpression.ArgumentList.Arguments.Count + 1 /* Implicit `self' */
                );

                break;
            }

            default:
                throw new InvalidOperationException(
                    $"Unsupported yield invocation type: {yieldExpression.Invocation.GetType().FullName}"
                );
        }

        Chunk.CodeGenerator.Emit(OpCode.YRET);
    }
}