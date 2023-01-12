using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(DecrementationExpression decrementationExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            var tgt = decrementationExpression.Target;

            if (decrementationExpression.IsPrefix)
            {
                Visit(tgt);
                cg.Emit(OpCode.DUP);
                EmitConstantLoadSequence(cg, 1);
                cg.Emit(OpCode.SUB);
            }
            else
            {
                Visit(tgt);
                EmitConstantLoadSequence(cg, 1);
                cg.Emit(OpCode.SUB);
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