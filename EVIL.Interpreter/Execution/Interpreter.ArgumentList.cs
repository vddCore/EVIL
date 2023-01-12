using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ArgumentListNode argumentListNode)
        {
            var arguments = new FunctionArguments();

            for (var i = 0; i < argumentListNode.Arguments.Count; i++)
            {
                arguments.Add(Visit(argumentListNode.Arguments[i]));
            }

            _argumentStack.Push(arguments);

            return DynValue.Zero;
        }
    }
}