namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SelfFnExpression selfFnExpression)
    {
        var id = InAnonymousSubChunkDo(() =>
        {
            InNewClosedScopeDo(() =>
            {
                Chunk.DebugDatabase.DefinedOnLine = selfFnExpression.Line;

                Chunk.MarkSelfAware();
                /* implicit `self' */ Chunk.AllocateParameter();

                if (selfFnExpression.ParameterList != null)
                {
                    Visit(selfFnExpression.ParameterList);
                }

                Visit(selfFnExpression.Statement);
                    
                FinalizeChunk();
            });
        });

        Chunk.CodeGenerator.Emit(
            OpCode.LDCNK,
            id
        );
    }
}