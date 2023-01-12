using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(Program program)
        {
            for (var i = 0; i < program.Statements.Count; i++)
            {
                Visit(program.Statements[i]);
            }
        }
    }
}