using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(SelfFnExpression selfFnExpression)
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
}