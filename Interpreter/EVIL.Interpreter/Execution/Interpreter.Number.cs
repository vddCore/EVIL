using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(NumberConstant numberConstant)
        {
            return new(numberConstant.Value);
        }
    }
}