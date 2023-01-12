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
                return VariableAssignment(varNode, assignmentNode.Right, assignmentNode.LocalScope);
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

        private DynValue VariableAssignment(VariableNode left, AstNode right, bool isLocal)
        {
            var retVal = Visit(right);

            if (isLocal)
            {
                if (Environment.IsInScriptFunctionScope)
                {
                    return Environment.LocalScope.Set(left.Name, Visit(right));
                }

                throw new RuntimeException("Local variable assignment outside of a function.", left.Line);
            }
            else if (Environment.IsInScriptFunctionScope)
            {
                if (Environment.LocalScope.HasMember(left.Name))
                {
                    Environment.LocalScope.Set(left.Name, retVal);
                    return retVal;
                }
            }

            retVal = Visit(right);
            Environment.GlobalScope.Set(left.Name, retVal);

            return retVal;
        }
    }
}