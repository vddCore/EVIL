using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(NullConstant nullConstant)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.LDNUL);
        }
    }
}