using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(ExitStatement exitStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.HLT);
        }
    }
}