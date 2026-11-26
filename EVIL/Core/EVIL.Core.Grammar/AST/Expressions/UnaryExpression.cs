namespace EVIL.Core.Grammar.AST.Expressions;

using System.Globalization;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Constants;

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
                    return new StringConstant(booleanConstant.Value.ToString().ToLowerInvariant(), false)
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
                    if (double.TryParse(stringConstant.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        return new NumberConstant(value)
                            .CopyMetadata<NumberConstant>(this);
                    }

                    break;
                }

                case UnaryOperationType.ToString:
                    return new StringConstant(
                        stringConstant.Value,
                        stringConstant.IsInterpolated,
                        stringConstant.Segments
                    )
                        .CopyMetadata<StringConstant>(this);

                case UnaryOperationType.LogicalNot:
                    return new BooleanConstant(false)
                        .CopyMetadata<BooleanConstant>(this);
            }
        }

        if (right is NilConstant)
        {
            switch (Type)
            {
                case UnaryOperationType.LogicalNot:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);

                case UnaryOperationType.ToString:
                    return new StringConstant("nil", false)
                        .CopyMetadata<StringConstant>(this);
            }
        }

        if (right is TypeCodeConstant typeCodeConstant)
        {
            switch (Type)
            {
                case UnaryOperationType.LogicalNot:
                    return new BooleanConstant(false)
                        .CopyMetadata<BooleanConstant>(this);

                case UnaryOperationType.ToString:
                    return new StringConstant(typeCodeConstant.Value.ToString(), false)
                        .CopyMetadata<StringConstant>(this);
            }
        }

        if (right != Right)
        {
            return new UnaryExpression(right, Type)
                .CopyMetadata<UnaryExpression>(this);
        }

        return this;
    }
}