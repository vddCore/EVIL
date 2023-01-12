using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler 
    {
        public override void Visit(DoWhileStatement doWhileStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var startLabel = CurrentChunk.DefineLabel(cg.IP);
            var continueLabel = CurrentChunk.DefineLabel();
            var endLabel = CurrentChunk.DefineLabel();
            
            LoopContinueLabels.Push(continueLabel);
            LoopEndLabels.Push(endLabel);
            
            Visit(doWhileStatement.Statement);
            CurrentChunk.UpdateLabel(continueLabel, cg.IP);
            Visit(doWhileStatement.Condition);
            cg.Emit(OpCode.TJMP, startLabel);
            CurrentChunk.UpdateLabel(endLabel, cg.IP);
        }
    }
}