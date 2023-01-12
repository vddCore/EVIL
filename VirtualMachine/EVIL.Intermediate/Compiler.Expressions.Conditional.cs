using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(ConditionalExpression conditionalExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var falseLabel = CurrentChunk.DefineLabel();
            var endLabel = CurrentChunk.DefineLabel();
            Visit(conditionalExpression.Condition);
            cg.Emit(OpCode.FJMP, falseLabel);
            Visit(conditionalExpression.TrueExpression);
            cg.Emit(OpCode.JUMP, endLabel);
            CurrentChunk.UpdateLabel(falseLabel, cg.IP);
            Visit(conditionalExpression.FalseExpression);
            CurrentChunk.UpdateLabel(endLabel, cg.IP);
        }
    }
}