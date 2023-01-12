using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(CompoundAssignmentNode compoundAssignmentNode)
        {
            var name = compoundAssignmentNode.Variable.Name;
            var val = Visit(compoundAssignmentNode.Right);

            if (Environment.IsInScriptFunctionScope)
            {
                var stackTop = Environment.CallStackTop;

                if (stackTop.HasParameter(name))
                {
                    var parameter = stackTop.Parameters[compoundAssignmentNode.Variable.Name];
                    stackTop.SetParameter(
                        name,
                        HandleCompoundOperation(
                            parameter,
                            val,
                            compoundAssignmentNode.Operation
                        )
                    );

                    return DynValue.Zero;
                }
            }

            var dynValue = Environment.LocalScope.FindInScopeChain(compoundAssignmentNode.Variable.Name);

            if (dynValue == null)
            {
                throw new RuntimeException(
                    $"Variable '{compoundAssignmentNode.Variable.Name}' was not found in current scope.", null);
            }

            dynValue.CopyFrom(
                HandleCompoundOperation(dynValue, val, compoundAssignmentNode.Operation)
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

                case CompoundAssignmentType.BitwiseAnd:
                    return new DynValue((int)variable.Number & (int)operand.Number);

                case CompoundAssignmentType.BitwiseOr:
                    return new DynValue((int)variable.Number | (int)operand.Number);

                case CompoundAssignmentType.BitwiseXor:
                    return new DynValue((int)variable.Number ^ (int)operand.Number);

                default:
                    throw new RuntimeException("Unexpected compound assignment type??", null);
            }
        }
    }
}