using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler 
    {
        public override void Visit(DoWhileStatement doWhileStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var startLabel = _executable.DefineLabel(cg.IP);
            var continueLabel = _executable.DefineLabel();
            var endLabel = _executable.DefineLabel();
            
            LoopContinueLabels.Push(continueLabel);
            LoopEndLabels.Push(endLabel);
            
            Visit(doWhileStatement.Statement);
            _executable.UpdateLabel(continueLabel, cg.IP);
            Visit(doWhileStatement.Condition);
            cg.Emit(OpCode.TJMP, startLabel);
            _executable.UpdateLabel(endLabel, cg.IP);
        }
    }
}