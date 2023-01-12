using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(IncrementationExpression incrementationExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            var tgt = incrementationExpression.Target;

            if (incrementationExpression.IsPrefix)
            {
                Visit(tgt);
                cg.Emit(OpCode.DUP);
                EmitConstantLoadSequence(cg, 1);
                cg.Emit(OpCode.ADD);
            }
            else
            {
                Visit(tgt);
                EmitConstantLoadSequence(cg, 1);
                cg.Emit(OpCode.ADD);
                cg.Emit(OpCode.DUP);
            }
            
            if (tgt is VariableReferenceExpression varRef)
            {
                EmitVariableStoreSequence(cg, varRef);
            }
            
            //todo indexed
        }
    }
}