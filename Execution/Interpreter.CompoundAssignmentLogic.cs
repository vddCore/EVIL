using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(CompoundAssignmentNode compoundAssignmentNode)
        {
            DynValue val;

            if (CallStack.Count > 0)
            {
                var stackTop = CallStack.Peek();
                val = Visit(compoundAssignmentNode.Right);

                if (stackTop.ParameterScope.ContainsKey(compoundAssignmentNode.Variable.Name))
                {

                    stackTop.ParameterScope[compoundAssignmentNode.Variable.Name] = HandleCompoundOperation(
                        stackTop.ParameterScope[compoundAssignmentNode.Variable.Name],
                        val,
                        compoundAssignmentNode.Operation
                    );

                    return DynValue.Zero;
                }
                else if (stackTop.LocalVariableScope.ContainsKey(compoundAssignmentNode.Variable.Name))
                {
                    stackTop.LocalVariableScope[compoundAssignmentNode.Variable.Name] = HandleCompoundOperation(
                        stackTop.LocalVariableScope[compoundAssignmentNode.Variable.Name],
                        val,
                        compoundAssignmentNode.Operation
                    );

                    return DynValue.Zero;
                }
            }

            if (!Environment.Globals.ContainsKey(compoundAssignmentNode.Variable.Name))
                throw new RuntimeException($"Variable '{compoundAssignmentNode.Variable.Name}' was not found in current scope.", null);

            val = Visit(compoundAssignmentNode.Right);

            Environment.Globals[compoundAssignmentNode.Variable.Name] = HandleCompoundOperation(
                Environment.Globals[compoundAssignmentNode.Variable.Name],
                val,
                compoundAssignmentNode.Operation
            );

            return DynValue.Zero;
        }

        private DynValue HandleCompoundOperation(DynValue variable, DynValue operand, CompoundAssignmentType type)
        {
            switch (type)
            {
                case CompoundAssignmentType.Add:
                    return new DynValue(variable.Number + operand.Number);

                case CompoundAssignmentType.Subtract:
                    return new DynValue(variable.Number - operand.Number);

                case CompoundAssignmentType.Multiply:
                    return new DynValue(variable.Number * operand.Number);

                case CompoundAssignmentType.Divide:
                    if (operand.Number == 0)
                        throw new RuntimeException("Cannot divide by zero.", null);

                    return new DynValue(variable.Number / operand.Number);

                case CompoundAssignmentType.Modulo:
                    return new DynValue(variable.Number % operand.Number);

                default:
                    throw new RuntimeException("Unexpected compound assignment type??", null);
            }
        }
    }
}
