using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ProgramNode programNode)
        {
            var retVal = DynValue.Zero;
            
            for (var i = 0; i < programNode.Statements.Count; i++)
            {
                retVal = Visit(programNode.Statements[i]);
            }

            return retVal;
        }
    }
}