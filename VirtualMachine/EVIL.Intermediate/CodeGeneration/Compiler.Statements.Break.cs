using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(BreakStatement breakStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.JUMP, LoopEndLabels.Peek());
        }
    }
}