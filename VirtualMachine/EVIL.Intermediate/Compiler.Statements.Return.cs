using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
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
            
            cg.Emit(OpCode.RETN);
        }    
    }
}