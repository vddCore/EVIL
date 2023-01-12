using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(ForStatement forStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            var startLabel = _executable.DefineLabel();
            var continueLabel = _executable.DefineLabel();
            var endLabel = _executable.DefineLabel();

            LoopContinueLabels.Push(continueLabel);
            LoopEndLabels.Push(endLabel);
            
            EnterScope();
            {
                foreach (var asgn in forStatement.Assignments)
                    Visit(asgn);
                
                _executable.UpdateLabel(startLabel, cg.IP);
                Visit(forStatement.Condition);
                cg.Emit(OpCode.FJMP, endLabel);
                Visit(forStatement.Statement);

                _executable.UpdateLabel(continueLabel, cg.IP);
                foreach (var expr in forStatement.IterationExpressions)
                {
                    Visit(expr);
                    cg.Emit(OpCode.POP);
                }

                cg.Emit(OpCode.JUMP, startLabel);
                _executable.UpdateLabel(endLabel, cg.IP);
            }

            LoopContinueLabels.Pop();
            LoopEndLabels.Pop();
            
            LeaveScope();
        }
    }
}