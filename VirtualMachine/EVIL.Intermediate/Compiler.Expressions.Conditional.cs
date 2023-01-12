using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(ConditionalExpression conditionalExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var falseLabel = _executable.DefineLabel();
            var endLabel = _executable.DefineLabel();
            Visit(conditionalExpression.Condition);
            cg.Emit(OpCode.FJMP, falseLabel);
            Visit(conditionalExpression.TrueExpression);
            cg.Emit(OpCode.JUMP, endLabel);
            _executable.UpdateLabel(falseLabel, cg.IP);
            Visit(conditionalExpression.FalseExpression);
            _executable.UpdateLabel(endLabel, cg.IP);
        }
    }
}