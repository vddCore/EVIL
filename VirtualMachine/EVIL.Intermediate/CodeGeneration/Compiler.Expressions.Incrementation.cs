using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(IncrementationExpression incrementationExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            var tgt = incrementationExpression.Target;
            
            if (tgt is VariableReferenceExpression varRef)
            {
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
                
                EmitVariableStore(cg, varRef);
            }
            else if (tgt is IndexerExpression indExpr)
            {
                if (incrementationExpression.IsPrefix)
                {
                    Visit(indExpr.Indexable);
                    Visit(indExpr.KeyExpression);
                    Visit(indExpr);
                    EmitConstantLoad(cg, 1);
                    cg.Emit(OpCode.ADD);
                    cg.Emit(OpCode.STE, (byte)1);
                }
                else
                {                   
                    Visit(indExpr);                    
                    Visit(indExpr.Indexable);
                    Visit(indExpr.KeyExpression);
                    
                    Visit(indExpr);
                    EmitConstantLoad(cg, 1);
                    cg.Emit(OpCode.ADD);
                    
                    cg.Emit(OpCode.STE, (byte)0);
                }
            }
            else
            {
                throw new CompilerException(
                    "Incrementation is only valid on variables and indexers.",
                    CurrentLine,
                    CurrentColumn
                );
            }
        }
    }
}