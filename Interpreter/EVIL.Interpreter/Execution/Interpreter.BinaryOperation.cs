using System;
using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BinaryExpression binaryExpression)
        {
            var left = Visit(binaryExpression.Left);
            var right = Visit(binaryExpression.Right);

            switch (binaryExpression.Type)
            {
                case BinaryOperationType.Plus:
                    return Addition(left, right, binaryExpression);

                case BinaryOperationType.Minus:
                    return Subtraction(left, right, binaryExpression);

                case BinaryOperationType.Multiply:
                    return Multiplication(left, right, binaryExpression);

                case BinaryOperationType.Divide:
                    return Division(left, right, binaryExpression);

                case BinaryOperationType.Modulo:
                    return Modulus(left, right, binaryExpression);

                case BinaryOperationType.ShiftLeft:
                    return ShiftLeft(left, right, binaryExpression);

                case BinaryOperationType.ShiftRight:
                    return ShiftRight(left, right, binaryExpression);

                case BinaryOperationType.BitwiseAnd:
                    return BitwiseAnd(left, right, binaryExpression);

                case BinaryOperationType.BitwiseOr:
                    return BitwiseOr(left, right, binaryExpression);

                case BinaryOperationType.BitwiseXor:
                    return BitwiseXor(left, right, binaryExpression);

                case BinaryOperationType.Less:
                    return CompareLess(left, right, binaryExpression);

                case BinaryOperationType.Greater:
                    return CompareGreater(left, right, binaryExpression);

                case BinaryOperationType.LessOrEqual:
                    return CompareLessOrEqual(left, right, binaryExpression);

                case BinaryOperationType.GreaterOrEqual:
                    return CompareGreaterOrEqual(left, right, binaryExpression);

                case BinaryOperationType.NotEqual:
                    return CompareNotEqual(left, right, binaryExpression);

                case BinaryOperationType.Equal:
                    return CompareEqual(left, right, binaryExpression);

                case BinaryOperationType.LogicalAnd:
                    return LogicalAnd(left, right);

                case BinaryOperationType.LogicalOr:
                    return LogicalOr(left, right);

                case BinaryOperationType.ExistsIn:
                    return ExistsIn(left, right, binaryExpression);

                default: throw new RuntimeException("Unknown binary operation.", Environment, binaryExpression.Line);
            }
        }

        private DynValue Addition(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String && right.Type == DynValueType.String)
                return new DynValue(left.String + right.String);
            else if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
                return new DynValue(left.Number + right.Number);
            else
            {
                throw new RuntimeException($"Attempt to add {left.Type} and {right.Type}.", Environment, node.Line);
            }
        }

        private DynValue Subtraction(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
                return new DynValue(left.Number - right.Number);
            else
            {
                throw new RuntimeException($"Attempt to subtract {left.Type} and {right.Type}.", Environment,
                    node.Line);
            }
        }

        private DynValue Multiplication(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number)
            {
                return new DynValue(left.Number * right.Number);
            }

            throw new RuntimeException(
                $"Attempt to multiply {left.Type} and {right.Type}.",
                Environment,
                node.Line);
        }

        private DynValue Division(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (right.Number == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(left.Number / right.Number);
            }
            else
            {
                throw new RuntimeException($"Attempt to divide {left.Type} and {right.Type}.", Environment, node.Line);
            }
        }

        private DynValue Modulus(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                if (right.Number == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(
                    left.Number - right.Number * Math.Floor(left.Number / right.Number)
                );
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt of modulo operation on {left.Type} by {right.Type}.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue ShiftLeft(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException(
                        $"Attempt to left-shift a string using {right.Type}.",
                        Environment,
                        node.Line
                    );
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
                return new DynValue((int)left.Number << (int)right.Number);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt of bitwise left-shift on {left.Type} using {right.Type}.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue ShiftRight(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException(
                        $"Attempt to right-shift a string using {right.Type}.",
                        Environment,
                        node.Line
                    );
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
                return new DynValue((int)left.Number >> (int)right.Number);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to right-shift a {left.Type} using {right.Type}.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue BitwiseAnd(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue((int)left.Number & (int)right.Number);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to bitwise AND {left.Type} and {right.Type}.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue BitwiseOr(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue((int)left.Number | (int)right.Number);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to bitwise OR {left.Type} and {right.Type}.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue BitwiseXor(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue((int)left.Number ^ (int)right.Number);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to bitwise XOR {left.Type} and {right.Type}.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue CompareLess(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number < right.Number);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new DynValue(left.String.Length < right.String.Length);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '<'.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue CompareGreater(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number > right.Number);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new DynValue(left.String.Length > right.String.Length);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '>'.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue CompareLessOrEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number <= right.Number);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new DynValue(left.String.Length <= right.String.Length);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '<='.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue CompareGreaterOrEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number >= right.Number);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new DynValue(left.String.Length >= right.String.Length);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '>='.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue ExistsIn(DynValue left, DynValue right, AstNode node)
        {
            if (right.Type == DynValueType.Table)
            {
                return new(right.Table[left] != null);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new(right.String.Contains(left.String));
            }

            throw new RuntimeException(
                $"Attempt to check existence of {left.Type} in {right.Type}.",
                Environment,
                node.Line
            );
        }

        private DynValue CompareNotEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number != right.Number);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new DynValue(left.String != right.String);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '!='.",
                    Environment,
                    node.Line
                );
            }
        }

        private DynValue CompareEqual(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
            {
                return new DynValue(left.Number == right.Number);
            }
            else if (left.Type == DynValueType.String && right.Type == DynValueType.String)
            {
                return new DynValue(left.String == right.String);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '=='.",
                    Environment,
                    node.Line
                );
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