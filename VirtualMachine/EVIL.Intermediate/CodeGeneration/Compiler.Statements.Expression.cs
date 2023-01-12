using EVIL.Grammar.AST;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(ExpressionStatement expressionStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            Visit(expressionStatement.Expression);
            cg.Emit(OpCode.POP);
        }
    }
}