using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnExpression fnExpression)
        {
            InNewClosedScopeDo(() =>
            {
                var id = InSubChunkDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnExpression.Line;

                    Visit(fnExpression.ParameterList);
                    Visit(fnExpression.Statement);
                });
                
                Chunk.CodeGenerator.Emit(
                    OpCode.LDCNK,
                    id
                );
            });
        }
    }
}