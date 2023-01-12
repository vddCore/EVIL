using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
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