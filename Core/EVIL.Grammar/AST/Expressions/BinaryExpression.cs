using System;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }

        public BinaryOperationType Type { get; }

        public BinaryExpression(Expression left, Expression right, BinaryOperationType type)
        {
            Left = left;
            Right = right;

            Type = type;

            Reparent(Left, Right);
        }

        public override Expression Reduce()
        {
            Expression ret = this;

            var left = Left.Reduce();
            var right = Right.Reduce();

            if (left is NumberConstant lnc)
            {
                if (right is NumberConstant rnc)
                {
                    switch (Type)
                    {
                        case BinaryOperationType.Divide when rnc.Value != 0:
                            return new NumberConstant(lnc.Value / rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Modulo when rnc.Value != 0:
                            return new NumberConstant(lnc.Value - rnc.Value * Math.Floor(lnc.Value / rnc.Value))
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Equal:
                            return new NumberConstant(lnc.Value == rnc.Value ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Greater:
                            return new NumberConstant(lnc.Value >= rnc.Value ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Less:
                            return new NumberConstant(lnc.Value < rnc.Value ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Subtract:
                            return new NumberConstant(lnc.Value - rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Multiply:
                            return new NumberConstant(lnc.Value * rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.Add:
                            return new NumberConstant(lnc.Value + rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.BitwiseAnd:
                            return new NumberConstant((long)lnc.Value & (long)rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.BitwiseOr:
                            return new NumberConstant((long)lnc.Value | (long)rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.BitwiseXor:
                            return new NumberConstant((long)lnc.Value ^ (long)rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.LogicalAnd:
                            return new NumberConstant(lnc.Value != 0 && rnc.Value != 0 ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.LogicalOr:
                            return new NumberConstant(lnc.Value != 0 || rnc.Value != 0 ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.NotEqual:
                            return new NumberConstant(lnc.Value != rnc.Value ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.ShiftLeft:
                            return new NumberConstant((long)lnc.Value << (int)rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.ShiftRight:
                            return new NumberConstant((long)lnc.Value >> (int)rnc.Value)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.GreaterOrEqual:
                            return new NumberConstant(lnc.Value >= rnc.Value ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);

                        case BinaryOperationType.LessOrEqual:
                            return new NumberConstant(lnc.Value <= rnc.Value ? 1 : 0)
                                .CopyMetadata<NumberConstant>(this);
                    }
                }
            }
            else if (left is StringConstant lsc)
            {
                if (right is StringConstant rsc)
                {
                    if (Type == BinaryOperationType.Add)
                    {
                        return new StringConstant(lsc.Value + rsc.Value)
                            .CopyMetadata<StringConstant>(this);
                    }
                }
                else if (right is NumberConstant rnc)
                {
                    if (Type == BinaryOperationType.ShiftLeft)
                    {
                        var str = lsc.Value;
                        var amount = (int)rnc.Value;

                        if (amount >= str.Length)
                        {
                            return new StringConstant(string.Empty)
                                .CopyMetadata<StringConstant>(this);
                        }

                        return new StringConstant(str.Substring(amount))
                            .CopyMetadata<StringConstant>(this);
                    }
                    else if (Type == BinaryOperationType.ShiftRight)
                    {
                        var str = lsc.Value;
                        var amount = str.Length - (int)rnc.Value;

                        if (amount <= 0)
                        {
                            return new StringConstant(string.Empty)
                                .CopyMetadata<StringConstant>(this);
                        }

                        return new StringConstant(str.Substring(0, amount))
                            .CopyMetadata<StringConstant>(this);
                    }
                }
            }

            return ret;
        }
    }
}