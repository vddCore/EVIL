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
                EmitConstantLoad(cg, 1);
                cg.Emit(OpCode.ADD);
                cg.Emit(OpCode.DUP);
            }
            else
            {
                Visit(tgt);
                cg.Emit(OpCode.DUP);
                EmitConstantLoad(cg, 1);
                cg.Emit(OpCode.ADD);
            }
            
            if (tgt is VariableReferenceExpression varRef)
            {
                EmitVariableStore(cg, varRef);
            }
            
            //todo indexed
        }
    }
}