﻿using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(AssignmentNode assignmentNode)
        {
            DynValue val;

            if (CallStack.Count > 0)
            {
                var stackTop = CallStack.Peek();

                if (stackTop.ParameterScope.ContainsKey(assignmentNode.Variable.Name))
                {
                    val = Visit(assignmentNode.Right);
                    stackTop.ParameterScope[assignmentNode.Variable.Name] = val;

                    return val;
                }
            }

            if (assignmentNode.LocalScope)
            {
                if (CallStack.Count > 0)
                {
                    var stackTop = CallStack.Peek();
                    val = Visit(assignmentNode.Right);

                    if (stackTop.LocalVariableScope.ContainsKey(assignmentNode.Variable.Name))
                        stackTop.LocalVariableScope[assignmentNode.Variable.Name] = val;
                    else
                        stackTop.LocalVariableScope.Add(assignmentNode.Variable.Name, val);

                    return val;
                }

                throw new RuntimeException("Local variable assignment outside of a function.", assignmentNode.Line);
            }
            else if (CallStack.Count > 0)
            {
                val = Visit(assignmentNode.Right);

                var stackTop = CallStack.Peek();

                if (stackTop.LocalVariableScope.ContainsKey(assignmentNode.Variable.Name))
                {
                    stackTop.LocalVariableScope[assignmentNode.Variable.Name] = val;
                    return val;
                }
            }

            if (!Environment.Globals.ContainsKey(assignmentNode.Variable.Name))
                Environment.Globals.Add(assignmentNode.Variable.Name, DynValue.Zero);

            val = Visit(assignmentNode.Right);
            Environment.Globals[assignmentNode.Variable.Name] = val;

            return val;
        }
    }
}
