using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(ReturnStatement returnStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (returnStatement.Expression != null)
            {
                Visit(returnStatement.Expression);
            }
            else
            {
                EmitConstantLoad(cg, 0);
            }

            if (cg.LastOpCode != OpCode.TCALL)
            {
                cg.Emit(OpCode.RETN);
            }
        }
    }
}