﻿namespace EVIL.Grammar.AST.Expressions;

using System.Globalization;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;

public sealed class UnaryExpression : Expression
{
    public Expression Right { get; }
    public UnaryOperationType Type { get; }

    public UnaryExpression(Expression right, UnaryOperationType type)
    {
        Right = right;
        Type = type;

        Reparent(Right);
    }

    public override Expression Reduce()
    {
        var right = Right.Reduce();

        if (right is NumberConstant numberConstant)
        {
            switch (Type)
            {
                case UnaryOperationType.Minus:
                    return new NumberConstant(-numberConstant.Value)
                        .CopyMetadata<NumberConstant>(this);
                    
                case UnaryOperationType.Plus:
                    return new NumberConstant(numberConstant.Value)
                        .CopyMetadata<NumberConstant>(this);
                    
                case UnaryOperationType.LogicalNot:
                    return new BooleanConstant(numberConstant.Value == 0)
                        .CopyMetadata<BooleanConstant>(this);

                case UnaryOperationType.BitwiseNot:
                    return new NumberConstant(~(long)numberConstant.Value)
                        .CopyMetadata<NumberConstant>(this);

                case UnaryOperationType.ToNumber:
                    return new NumberConstant(numberConstant.Value)
                        .CopyMetadata<NumberConstant>(this);

                case UnaryOperationType.ToString:
                    return new StringConstant(numberConstant.Value.ToString(CultureInfo.InvariantCulture), false)
                        .CopyMetadata<StringConstant>(this);
            }
        }

        if (right is BooleanConstant booleanConstant)
        {
            switch (Type)
            {
                case UnaryOperationType.LogicalNot:
                    return new BooleanConstant(!booleanConstant.Value)
                        .CopyMetadata<BooleanConstant>(this);
                
                case UnaryOperationType.ToNumber:
                    return new NumberConstant(booleanConstant.Value ? 1 : 0)
                        .CopyMetadata<NumberConstant>(this);
                
                case UnaryOperationType.ToString:
                    return new StringConstant(booleanConstant.Value.ToString(CultureInfo.InvariantCulture).ToLower(), false)
                        .CopyMetadata<StringConstant>(this);
            }
        }

        if (right is StringConstant stringConstant)
        {
            switch (Type)
            {
                case UnaryOperationType.Length:
                    return new NumberConstant(stringConstant.Value.Length)
                        .CopyMetadata<NumberConstant>(this);

                case UnaryOperationType.ToNumber:
                {
                    if (double.TryParse(stringConstant.Value, out var value))
                    {
                        return new NumberConstant(value)
                            .CopyMetadata<NumberConstant>(this);
                    }

                    break;
                }

                case UnaryOperationType.ToString:
                    return new StringConstant(stringConstant.Value, stringConstant.IsInterpolated)
                        .CopyMetadata<StringConstant>(this);
            }
        }

        return this;
    }
}