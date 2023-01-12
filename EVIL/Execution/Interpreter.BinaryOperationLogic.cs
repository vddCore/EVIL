using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVIL.Abstraction;
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BinaryOperationNode binaryOperationNode)
        {
            var left = Visit(binaryOperationNode.LeftOperand);
            var right = Visit(binaryOperationNode.RightOperand);

            switch (binaryOperationNode.Type)
            {
                case BinaryOperationType.Plus:
                    return Addition(left, right, binaryOperationNode);

                case BinaryOperationType.Minus:
                    return Subtraction(left, right, binaryOperationNode);

                case BinaryOperationType.Multiply:
                    if (left.Type == DynValueType.Number)
                        return Multiplication(right, left, binaryOperationNode);
                    else
                        return Multiplication(left, right, binaryOperationNode);

                case BinaryOperationType.Divide:
                    return Division(left, right, binaryOperationNode);

                case BinaryOperationType.Modulo:
                    return Modulus(left, right, binaryOperationNode);

                case BinaryOperationType.ShiftLeft:
                    return ShiftLeft(left, right, binaryOperationNode);

                case BinaryOperationType.ShiftRight:
                    return ShiftRight(left, right, binaryOperationNode);

                case BinaryOperationType.BitwiseAnd:
                    return BitwiseAnd(left, right, binaryOperationNode);

                case BinaryOperationType.BitwiseOr:
                    return BitwiseOr(left, right, binaryOperationNode);

                case BinaryOperationType.BitwiseXor:
                    return BitwiseXor(left, right, binaryOperationNode);

                case BinaryOperationType.Less:
                    return CompareLess(left, right, binaryOperationNode);

                case BinaryOperationType.Greater:
                    return CompareGreater(left, right, binaryOperationNode);

                case BinaryOperationType.LessOrEqual:
                    return CompareLessOrEqual(left, right, binaryOperationNode);

                case BinaryOperationType.GreaterOrEqual:
                    return CompareGreaterOrEqual(left, right, binaryOperationNode);

                case BinaryOperationType.NotEqual:
                    return CompareNotEqual(left, right, binaryOperationNode);

                case BinaryOperationType.Equal:
                    return CompareEqual(left, right, binaryOperationNode);

                case BinaryOperationType.And:
                    return LogicalAnd(left, right);

                case BinaryOperationType.Or:
                    return LogicalOr(left, right);

                default: throw new RuntimeException("Unknown binary operation.", binaryOperationNode.Line);
            }
        }

        private DynValue Addition(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String && right.Type == DynValueType.String)
                return new DynValue(left.String + right.String);
            else if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
                return new DynValue(left.Number + right.Number);
            else if (left.Type == DynValueType.Function && right.Type == DynValueType.Function)
            {
                if (left.IsClrFunction || right.IsClrFunction)
                    throw new RuntimeException("Mixing CLR and EVIL functions is not supported.", node.Line);

                var stmts = new List<AstNode>();
                stmts.AddRange(left.ScriptFunction.StatementList);
                stmts.AddRange(right.ScriptFunction.StatementList);

                var parameters = new List<string>();
                parameters.AddRange(left.ScriptFunction.ParameterNames);
                parameters.AddRange(right.ScriptFunction.ParameterNames);
                parameters = parameters.Distinct().ToList();

                return new DynValue(new ScriptFunction(stmts, parameters));
            }
            else
            {
                throw new RuntimeException($"Attempt to add {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue Subtraction(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number - right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to subtract {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue Multiplication(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException($"Attempt to repeat a string using {right.Type}.", node.Line);
                }

                var sb = new StringBuilder();
                for (var i = 0; i < right.Number; i++)
                {
                    sb.Append(left.String);
                }

                return new DynValue(sb.ToString());
            }
            else if (left.Type == DynValueType.Function)
            {
                if (left.IsClrFunction)
                    throw new RuntimeException($"CLR function invocations cannot be multiplied.", node.Line);

                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException($"Attempt to multiply function using {right.Type}.", node.Line);
                }

                var stmts = new List<AstNode>();
                var parameters = new List<string>(left.ScriptFunction.ParameterNames);

                for (var i = 0; i < right.Number; i++)
                {
                    stmts.AddRange(left.ScriptFunction.StatementList);
                }

                return new DynValue(new ScriptFunction(stmts, parameters));
            }
            else if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number * right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to multiply {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue Division(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (right.Number == 0)
                    throw new RuntimeException("Attempt to divide by zero.", node.Line);

                return new DynValue(left.Number / right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to divide {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue Modulus(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (right.Number == 0)
                    throw new RuntimeException("Attempt to divide by zero.", node.Line);

                return new DynValue(left.Number % right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt of modulo operation on {left.Type} by {right.Type}.", node.Line);
            }
        }

        private DynValue ShiftLeft(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException($"Attempt to left-shift a string using {right.Type}.", node.Line);
                }

                if (right.Number > left.String.Length)
                    return new DynValue(string.Empty);
                else if (right.Number < 0)
                {
                    var prefix = new string(' ', (int)Math.Abs(right.Number));
                    return new DynValue(prefix + left.String);
                }

                return new DynValue(left.String[(int)right.Number..left.String.Length]);
            }
            else if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                    throw new RuntimeException("Left-shift operation is only allowed on integer numbers.", node.Line);

                return new DynValue((int)left.Number << (int)right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt of bitwise left-shift on {left.Type} using {right.Type}.",
                    node.Line);
            }
        }

        private DynValue ShiftRight(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException($"Attempt to right-shift a string using {right.Type}.", node.Line);
                }

                if (right.Number > left.String.Length)
                    return new DynValue(string.Empty);
                else if (right.Number < 0)
                {
                    var prefix = new string(' ', (int)Math.Abs(right.Number));
                    return new DynValue(left.String + prefix);
                }
                return new DynValue(left.String[0..(left.String.Length - (int)right.Number)]);
            }
            else if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                    throw new RuntimeException($"Attempt to right-shift a non-integral number.", node.Line);

                return new DynValue((int)left.Number >> (int)right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to right-shift a {left.Type} using {right.Type}.", node.Line);
            }
        }

        private DynValue BitwiseAnd(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                    throw new RuntimeException($"Attempt to bitwise AND a non-integral number.", node.Line);

                return new DynValue((int)left.Number & (int)right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to bitwise AND {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue BitwiseOr(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                    throw new RuntimeException($"Attempt to bitwise OR a non-integral number.", node.Line);

                return new DynValue((int)left.Number | (int)right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to bitwise OR {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue BitwiseXor(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                    throw new RuntimeException($"Attempt to bitwise XOR a non-integral number.", node.Line);

                return new DynValue((int)left.Number ^ (int)right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to bitwise XOR {left.Type} and {right.Type}.", node.Line);
            }
        }

        private DynValue CompareLess(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number < right.Number)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                if (left.String.Length < right.String.Length)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else
            {
                throw new RuntimeException($"Attempt to compare {left.Type} and {right.Type} using '>'.", node.Line);
            }
        }

        private DynValue CompareGreater(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number > right.Number)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                if (left.String.Length > right.String.Length)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else
            {
                throw new RuntimeException($"Attempt to compare {left.Type} and {right.Type} using '<'.", node.Line);
            }
        }

        private DynValue CompareLessOrEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number <= right.Number)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                if (left.String.Length <= right.String.Length)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else
            {
                throw new RuntimeException($"Attempt to compare {left.Type} and {right.Type} using '<='.", node.Line);
            }
        }

        private DynValue CompareGreaterOrEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number >= right.Number)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                if (left.String.Length >= right.String.Length)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else
            {
                throw new RuntimeException($"Attempt to compare {left.Type} and {right.Type} using '>='.", node.Line);
            }
        }

        private DynValue CompareNotEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number != right.Number)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                if (left.String != right.String)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else
            {
                throw new RuntimeException($"Attempt to compare {left.Type} and {right.Type} using '!='.", node.Line);
            }
        }

        private DynValue CompareEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (left.Number == right.Number)
                    return new DynValue(1);
                else return new DynValue(0);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                if (left.String == right.String)
                    return new DynValue(1);

                else return new DynValue(0);
            }
            else
            {
                throw new RuntimeException($"Attempt to compare {left.Type} and {right.Type} using '=='.", node.Line);
            }
        }

        private DynValue LogicalAnd(DynValue left, DynValue right)
        {
            if (left.IsTruth && right.IsTruth)
            {
                return right.Copy();
            }
            else
            {
                return new DynValue(0);
            }
        }

        private DynValue LogicalOr(DynValue left, DynValue right)
        {
            if (left.IsTruth)
            {
                return left.Copy();
            }
            else if (right.IsTruth)
            {
                return right.Copy();
            }
            else return DynValue.Zero;
        }
    }
}