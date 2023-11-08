using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(SelfFnExpression selfFnExpression)
        {
            var id = InSubChunkDo(() =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = selfFnExpression.Line;

                    Chunk.MarkSelfAware();
                    /* implicit `self' */ Chunk.AllocateParameter();
                    
                    Visit(selfFnExpression.ParameterList);
                    Visit(selfFnExpression.Statement);
                });
            });

            Chunk.CodeGenerator.Emit(
                OpCode.LDCNK,
                id
            );
        }
    }
}