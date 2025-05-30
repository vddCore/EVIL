namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SelfInvocationExpression selfInvocationExpression)
    {
        Visit(selfInvocationExpression.Indexable);
        Visit(selfInvocationExpression.ArgumentList);

        Visit(selfInvocationExpression.Indexable);
            
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