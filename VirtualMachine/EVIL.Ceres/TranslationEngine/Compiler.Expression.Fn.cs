namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

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