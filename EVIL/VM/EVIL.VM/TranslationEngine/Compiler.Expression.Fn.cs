namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(FnExpression fnExpression)
    {
        var id = InAnonymousSubChunkDo(() =>
        {
            InNewClosedScopeDo(() =>
            {
                Chunk.DebugDatabase.DefinedOnLine = fnExpression.Line;

                if (fnExpression.ParameterList != null)
                {
                    Visit(fnExpression.ParameterList);
                }

                Visit(fnExpression.Statement);

                FinalizeChunk();
            });
        });

        Chunk.CodeGenerator.Emit(
            OpCode.LDCNK,
            id
        );
    }
}