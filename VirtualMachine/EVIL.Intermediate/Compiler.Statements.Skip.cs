using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(SkipStatement nextStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.JUMP, LoopContinueLabels.Peek());
        }
    }
}