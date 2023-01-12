using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(WhileStatement whileStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var startLabel = _executable.DefineLabel(cg.IP);
            var endLabel = _executable.DefineLabel();
            
            LoopContinueLabels.Push(startLabel);
            LoopEndLabels.Push(endLabel);
            
            Visit(whileStatement.Expression);
            cg.Emit(OpCode.FJMP, endLabel);
            Visit(whileStatement.Statement);
            cg.Emit(OpCode.JUMP, startLabel);
            _executable.UpdateLabel(endLabel, cg.IP);
        }
    }
}