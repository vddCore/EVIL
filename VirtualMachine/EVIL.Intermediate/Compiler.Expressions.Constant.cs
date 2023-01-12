using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(NumberConstant numberConstant)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            EmitConstantLoad(cg, numberConstant.Value);
        }

        public override void Visit(StringConstant stringConstant)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            EmitConstantLoad(cg, stringConstant.Value);
        }
    }
}