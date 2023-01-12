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
            else if (left.Type == DynValueType.Number && right.Type == DynValueType.Number)
                return new DynValue(left.Number + right.Number);
            else if (left.Type == DynValueType.Function && right.Type == DynValueType.Function)
            {
                var stmts = new List<AstNode>();
                stmts.AddRange(left.ScriptFunction.StatementList);
                stmts.AddRange(right.ScriptFunction.StatementList);

                var parameters = new List<string>();
                var leftParameters = left.ScriptFunction.ParameterNames;
                var rightParameters = right.ScriptFunction.ParameterNames;
                
                for (var i = 0; i < leftParameters.Count; i++)
                {
                    parameters.Add(leftParameters[i]);
                }

                for (var i = 0; i < rightParameters.Count; i++)
                {
                    var param = rightParameters[i];
                    
                    if (!parameters.Contains(param))
                        parameters.Add(param);
                }

                return new DynValue(new ScriptFunction(stmts, parameters, node.Line));
            }
            else
            {
                throw new RuntimeException($"Attempt to add {left.Type} and {right.Type}.", Environment, node.Line);
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
                throw new RuntimeException($"Attempt to subtract {left.Type} and {right.Type}.", Environment,
                    node.Line);
            }
        }

        private DynValue Multiplication(DynValue left, DynValue right, AstNode node)
        {
            if (left.Type == DynValueType.String)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException($"Attempt to repeat a string using {right.Type}.", Environment,
                        node.Line);
                }

                return new DynValue(RepeatString(left.String, right.Number));
            }
            else if (left.Type == DynValueType.Function)
            {
                if (right.Type != DynValueType.Number)
                {
                    throw new RuntimeException($"Attempt to multiply a function using {right.Type}.", Environment,
                        node.Line);
                }

                return new DynValue(RepeatFunction(left.ScriptFunction, right.Number, node));
            }
            else if (left.Type == DynValueType.Number)
            {
                if (right.Type == DynValueType.Number)
                {
                    return new DynValue(left.Number * right.Number);
                }
                else if (right.Type == DynValueType.String)
                {
                    return new DynValue(RepeatString(right.String, left.Number));
                }
                else if (right.Type == DynValueType.Function)
                {
                    return new DynValue(RepeatFunction(right.ScriptFunction, left.Number, node));
                }
                else
                {
                    throw new RuntimeException($"Attempt to multiply {left.Type} and {right.Type}.", Environment,
                        node.Line);
                }
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to multiply {left.Type} and {right.Type}.",
                    Environment,
                    node.Line);
            }
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
                    left.Number - right.Number * decimal.Floor(left.Number / right.Number)
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
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                {
                    throw new RuntimeException(
                        "Left-shift operation is only allowed on integer numbers.",
                        Environment,
                        node.Line
                    );
                }

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
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                {
                    throw new RuntimeException(
                        $"Attempt to right-shift a non-integral number.",
                        Environment,
                        node.Line
                    );
                }

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
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                {
                    throw new RuntimeException(
                        $"Attempt to bitwise AND a non-integral number.",
                        Environment,
                        node.Line
                    );
                }

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
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                {
                    throw new RuntimeException(
                        $"Attempt to bitwise OR a non-integral number.",
                        Environment,
                        node.Line
                    );
                }

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
                if (left.Number % 1 != 0 || right.Number % 1 != 0)
                {
                    throw new RuntimeException(
                        $"Attempt to bitwise XOR a non-integral number.",
                        Environment,
                        node.Line
                    );
                }

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
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '>'.",
                    Environment,
                    node.Line
                );
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
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '<'.",
                    Environment,
                    node.Line
                );
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
                throw new RuntimeException(
                    $"Attempt to compare {left.Type} and {right.Type} using '>='.",
                    Environment,
                    node.Line
                );
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

        private DynValue ExistsIn(DynValue left, DynValue right, BinaryOperationNode node)
        {
            if (right.Type == DynValueType.Table)
            {
                if (left.Type == DynValueType.String || left.Type == DynValueType.Number)
                {
                    return new DynValue(right.Table.ContainsKey(left));
                }
            }
            else if (right.Type == DynValueType.String)
            {
                return new DynValue(right.String.Contains(left.AsString().String));
            }

            throw new RuntimeException(
                $"Attempt to check existence of {left.Type} in {right.Type}.", 
                Environment,
                node.Line
            );
        }

        private string RepeatString(string str, decimal repetitions)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < repetitions; i++)
                sb.Append(str);

            return sb.ToString();
        }

        private ScriptFunction RepeatFunction(ScriptFunction function, decimal repetitions, AstNode node)
        {
            var stmts = new List<AstNode>();
            var parameters = new List<string>(function.ParameterNames);

            for (var i = 0; i < repetitions; i++)
                stmts.AddRange(function.StatementList);

            return new ScriptFunction(stmts, parameters, node.Line);
        }
    }
}