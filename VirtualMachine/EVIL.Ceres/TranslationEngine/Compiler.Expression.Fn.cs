using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnExpression fnExpression)
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
}