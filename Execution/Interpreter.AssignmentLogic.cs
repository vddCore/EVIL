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
            DynValue val;

            if (CallStack.Count > 0)
            {
                var stackTop = CallStack.Peek();

                if (stackTop.ParameterScope.ContainsKey(left.Name))
                {
                    val = Visit(right);
                    stackTop.ParameterScope[left.Name] = val;

                    return val;
                }
            }

            if (isLocal)
            {
                if (CallStack.Count > 0)
                {
                    var stackTop = CallStack.Peek();
                    val = Visit(right);

                    if (stackTop.LocalVariableScope.ContainsKey(left.Name))
                        stackTop.LocalVariableScope[left.Name] = val;
                    else
                        stackTop.LocalVariableScope.Add(left.Name, val);

                    return val;
                }

                throw new RuntimeException("Local variable assignment outside of a function.", left.Line);
            }
            else if (CallStack.Count > 0)
            {
                val = Visit(right);

                var stackTop = CallStack.Peek();

                if (stackTop.LocalVariableScope.ContainsKey(left.Name))
                {
                    stackTop.LocalVariableScope[left.Name] = val;
                    return val;
                }
            }

            if (!Environment.Globals.ContainsKey(left.Name))
                Environment.Globals.Add(left.Name, DynValue.Zero);

            val = Visit(right);
            Environment.Globals[left.Name] = val;

            return val;
        }
    }
}