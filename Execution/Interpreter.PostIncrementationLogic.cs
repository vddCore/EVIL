using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(PostIncrementationNode postIncrementationNode)
        {
            DynValue retVal;
            var name = postIncrementationNode.Variable.Name;

            if (CallStack.Count > 0)
            {
                var stackTop = CallStack.Peek();

                if (stackTop.ParameterScope.ContainsKey(name))
                {
                    retVal = stackTop.ParameterScope[name].Copy();
                    stackTop.ParameterScope[name] = new DynValue(retVal.Number + 1);
                }
                else if (stackTop.LocalVariableScope.ContainsKey(name))
                {
                    retVal = stackTop.LocalVariableScope[name].Copy();
                    stackTop.LocalVariableScope[name] = new DynValue(retVal.Number + 1);
                }
                else if (Environment.Globals.ContainsKey(name))
                {
                    retVal = Environment.Globals[name].Copy();
                    Environment.Globals[name] = new DynValue(retVal.Number + 1);
                }
                else
                {
                    throw new RuntimeException($"The referenced variable '{name}' was never defined.",
                        postIncrementationNode.Line);
                }
            }
            else
            {
                if (!Environment.Globals.ContainsKey(name))
                    throw new RuntimeException($"The referenced variable '{name}' was never defined.",
                        postIncrementationNode.Line);

                retVal = Environment.Globals[name].Copy();
                Environment.Globals[name] = new DynValue(retVal.Number + 1);
            }

            return retVal;
        }
    }
}