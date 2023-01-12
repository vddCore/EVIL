using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(WhileStatement whileStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var startLabel = CurrentChunk.DefineLabel(cg.IP);
            var endLabel = CurrentChunk.DefineLabel();
            
            LoopContinueLabels.Push(startLabel);
            LoopEndLabels.Push(endLabel);
            
            Visit(whileStatement.Expression);
            cg.Emit(OpCode.FJMP, endLabel);
            Visit(whileStatement.Statement);
            cg.Emit(OpCode.JUMP, startLabel);
            CurrentChunk.UpdateLabel(endLabel, cg.IP);
        }
    }
}