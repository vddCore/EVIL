using EVIL.Abstraction;
using EVIL.AST.Base;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(AssignmentNode assignmentNode)
        {
            if (assignmentNode.Left is VariableNode varNode)
            {
                return VariableAssignment(varNode, assignmentNode.Right);
            }
            else if (assignmentNode.Left is IndexingNode)
            {
                var indexable = Visit(assignmentNode.Left);
                var newValue = Visit(assignmentNode.Right);

                indexable.CopyFrom(newValue);
                return indexable;
            }
            else
            {
                throw new RuntimeException("Unexpected assignment.", assignmentNode.Line);
            }
        }

        private DynValue VariableAssignment(VariableNode left, AstNode right)
        {
            var dynValue = Environment.LocalScope.FindInScopeChain(left.Identifier);

            if (dynValue == null)
            {
                throw new RuntimeException($"'{left.Identifier}' was not found in the current scope.", left.Line);
            }
            
            var retVal = Visit(right);
            dynValue.CopyFrom(retVal);

            return retVal;
        }
    }
}