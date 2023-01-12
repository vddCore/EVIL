using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(PostDecrementationNode postDecrementationNode)
        {
            var numValue = Visit(postDecrementationNode.Left);

            if (numValue.Type != DynValueType.Number)
            {
                throw new RuntimeException(
                    "Cannot decrement this value because it's not a number.", postDecrementationNode.Line
                );
            }
            
            var retVal = numValue.Copy();
            numValue.CopyFrom(new DynValue(numValue.Number - 1));

            return retVal;
        }
    }
}