using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(ForStatement forStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var startLabel = CurrentChunk.DefineLabel();
            var continueLabel = CurrentChunk.DefineLabel();
            var endLabel = CurrentChunk.DefineLabel();

            LoopContinueLabels.Push(continueLabel);
            LoopEndLabels.Push(endLabel);
            
            EnterScope();
            {
                foreach (var asgn in forStatement.Assignments)
                    Visit(asgn);
                
                CurrentChunk.UpdateLabel(startLabel, cg.IP);
                Visit(forStatement.Condition);
                cg.Emit(OpCode.FJMP, endLabel);
                Visit(forStatement.Statement);

                CurrentChunk.UpdateLabel(continueLabel, cg.IP);
                foreach (var expr in forStatement.IterationExpressions)
                {
                    Visit(expr);
                    cg.Emit(OpCode.POP);
                }

                cg.Emit(OpCode.JUMP, startLabel);
                CurrentChunk.UpdateLabel(endLabel, cg.IP);
            }

            LoopContinueLabels.Pop();
            LoopEndLabels.Pop();
            
            LeaveScope();
        }
    }
}