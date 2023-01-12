using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(CompoundAssignmentNode compoundAssignmentNode)
        {
            var name = compoundAssignmentNode.Variable.Identifier;
            var val = Visit(compoundAssignmentNode.Right);

            var dynValue = Environment.LocalScope.FindInScopeChain(name);

            if (dynValue == null)
            {
                throw new RuntimeException(
                    $"Variable '{name}' was not found in current scope.", null);
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