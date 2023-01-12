using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(FunctionCallExpression functionCallExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (functionCallExpression.Arguments.Count > 255)
            {
                throw new CompilerException(
                    "Upper argument limit (255) for a function call has been reached.\n" +
                    "Perhaps it is time to simplify your function.",
                    CurrentLine,
                    CurrentColumn
                );
            }
            
            foreach (var expr in functionCallExpression.Arguments)
                Visit(expr);

            Visit(functionCallExpression.Callee);
            cg.Emit(OpCode.CALL, (byte)functionCallExpression.Arguments.Count);
        }
    }
}