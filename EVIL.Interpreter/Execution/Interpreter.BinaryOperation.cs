using System;
using System.Collections.Generic;
using System.Text;
using EVIL.Grammar;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(BinaryOperationNode binaryOperationNode)
        {
            var left = Visit(binaryOperationNode.Left);
            var right = Visit(binaryOperationNode.Right);

            switch (binaryOperationNode.Type)
            {
                case BinaryOperationType.Plus:
                    return Addition(left, right, binaryOperationNode);

                case BinaryOperationType.Minus:
                    return Subtraction(left, right, binaryOperationNode);

                case BinaryOperationType.Multiply:
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
                
                case BinaryOperationType.ExistsIn:
                    return ExistsIn(left, right, binaryOperationNode);

                default: throw new RuntimeException("Unknown binary operation.", Environment, binaryOperationNode.Line);
            }
        }

        private DynValue Addition(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String && right.Type == DynValueType.String)
                return new DynValue(left.String + right.String);
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
                return new DynValue(left.Decimal + right.Decimal);
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
                return new DynValue(left.Integer + right.Decimal);
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
                return new DynValue(left.Decimal + right.Integer);
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
                return new DynValue(left.Integer + right.Integer);
            else
            {
                throw new RuntimeException($"Attempt to add {left.Type} and {right.Type}.", Environment, node.Line);
            }
        }

        private DynValue Subtraction(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
                return new DynValue(left.Decimal - right.Decimal);
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
                return new DynValue(left.Integer - right.Decimal);
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
                return new DynValue(left.Decimal - right.Integer);
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
                return new DynValue(left.Integer - right.Integer);
            else
            {
                throw new RuntimeException($"Attempt to subtract {left.Type} and {right.Type}.", Environment,
                    node.Line);
            }
        }

        private DynValue Multiplication(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Decimal)
            {
                if (right.Type == DynValueType.Decimal)
                {
                    return new DynValue(left.Decimal * right.Decimal);
                }
                else if (right.Type == DynValueType.Integer)
                {
                    return new DynValue(left.Decimal * right.Integer);
                }
            }
            else if (left.Type == DynValueType.Integer)
            {
                if (right.Type == DynValueType.Integer)
                {
                    return new DynValue(left.Integer * right.Integer);
                }
                else if (right.Type == DynValueType.Decimal)
                {
                    return new DynValue(left.Integer * right.Decimal);
                }
            }

            throw new RuntimeException(
                $"Attempt to multiply {left.Type} and {right.Type}.",
                Environment,
                node.Line);
        }

        private DynValue Division(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                if (right.Decimal == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(left.Decimal / right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                if (right.Decimal == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(left.Integer / right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                if (right.Integer == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(left.Decimal / right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                if (right.Integer == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(left.Integer / right.Integer);
            }
            else
            {
                throw new RuntimeException($"Attempt to divide {left.Type} and {right.Type}.", Environment, node.Line);
            }
        }

        private DynValue Modulus(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                if (right.Decimal == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(
                    left.Decimal - right.Decimal * decimal.Floor(left.Decimal / right.Decimal)
                );
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                if (right.Decimal == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(
                    left.Integer - right.Decimal * decimal.Floor(left.Integer / right.Decimal)
                );
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                if (right.Integer == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(
                    left.Decimal - right.Integer * decimal.Floor(left.Decimal / right.Integer)
                );
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                if (right.Integer == 0)
                    throw new RuntimeException("Attempt to divide by zero.", Environment, node.Line);

                return new DynValue(
                    left.Integer - right.Integer * (left.Integer / right.Integer)
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
                if (right.Type != DynValueType.Integer)
                {
                    throw new RuntimeException(
                        $"Attempt to left-shift a string using {right.Type}.",
                        Environment,
                        node.Line
                    );
                }

                if (right.Integer > left.String.Length)
                    return new DynValue(string.Empty);
                else if (right.Integer < 0)
                {
                    var prefix = new string(' ', Math.Abs(right.Integer));
                    return new DynValue(prefix + left.String);
                }

                return new DynValue(left.String[right.Integer..left.String.Length]);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer << right.Integer);
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
                if (right.Type != DynValueType.Integer)
                {
                    throw new RuntimeException(
                        $"Attempt to right-shift a string using {right.Type}.",
                        Environment,
                        node.Line
                    );
                }

                if (right.Integer > left.String.Length)
                    return new DynValue(string.Empty);
                else if (right.Decimal < 0)
                {
                    var prefix = new string(' ', Math.Abs(right.Integer));
                    return new DynValue(left.String + prefix);
                }
                return new DynValue(left.String[0..(left.String.Length - right.Integer)]);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer >> right.Integer);
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
            if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer & right.Integer);
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
            if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer | right.Integer);
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
            if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer ^ right.Integer);
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
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Decimal < right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Integer < right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Decimal < right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer < right.Integer);
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
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Decimal > right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Integer > right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Decimal > right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer > right.Integer);
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
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Decimal <= right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Integer <= right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Decimal <= right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer <= right.Integer);
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
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Decimal >= right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Integer >= right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Decimal >= right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer >= right.Integer);
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
                return new(right.Table.GetKeyByDynValue(left) != null);
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
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Decimal != right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Integer != right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Decimal != right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer != right.Integer);
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
            if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Decimal == right.Decimal);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Decimal)
            {
                return new DynValue(left.Integer == right.Decimal);
            }
            else if (left.Type == DynValueType.Decimal && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Decimal == right.Integer);
            }
            else if (left.Type == DynValueType.Integer && right.Type == DynValueType.Integer)
            {
                return new DynValue(left.Integer == right.Integer);
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