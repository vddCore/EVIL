using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.XARGS);
        }
    }
}