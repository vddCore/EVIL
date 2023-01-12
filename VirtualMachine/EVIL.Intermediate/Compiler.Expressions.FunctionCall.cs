using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(FunctionCallExpression functionCallExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            foreach (var expr in functionCallExpression.Arguments)
                Visit(expr);

            Visit(functionCallExpression.Callee);
            cg.Emit(OpCode.CALL, functionCallExpression.Arguments.Count);
        }
    }
}