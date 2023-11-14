using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnExpression fnExpression)
        {
            var id = InSubChunkDo(() =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnExpression.Line;

                    Visit(fnExpression.ParameterList);
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
}