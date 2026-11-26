namespace EVIL.Core.Grammar.AST.Expressions;

using System;
using System.Collections.Generic;
using System.Globalization;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.StringInterpolation;

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

    private static bool? GetConstantTruthiness(Expression expr) => expr switch
    {
        BooleanConstant bc => bc.Value,
        NumberConstant nc => nc.Value != 0,
        NilConstant => false,
        StringConstant => true,
        TypeCodeConstant => true,
        _ => null
    };

    private static string? GetConstantStringRepresentation(Expression expr) => expr switch
    {
        StringConstant sc => sc.Value,
        NumberConstant nc => nc.Value.ToString(CultureInfo.InvariantCulture),
        BooleanConstant bc => bc.Value.ToString().ToLowerInvariant(),
        NilConstant => "nil",
        TypeCodeConstant tc => tc.Value.ToString(),
        _ => null
    };

    private static IReadOnlyList<InterpolatedSegment> GetStringSegments(StringConstant stringConstant)
    {
        if (stringConstant.IsInterpolated && stringConstant.Segments.Count > 0)
        {
            return stringConstant.Segments;
        }

        return [new InterpolatedSegment(InterpolatedSegmentKind.Literal, stringConstant.Value)];
    }

    private static IReadOnlyList<InterpolatedSegment> CombineStringSegments(
        IReadOnlyList<InterpolatedSegment> left,
        IReadOnlyList<InterpolatedSegment> right)
    {
        var segments = new List<InterpolatedSegment>(left.Count + right.Count);
        AppendSegments(segments, left);
        AppendSegments(segments, right);
        return segments;
    }

    private static void AppendSegments(
        List<InterpolatedSegment> target,
        IReadOnlyList<InterpolatedSegment> segments)
    {
        foreach (var segment in segments)
        {
            if (segment.Kind == InterpolatedSegmentKind.Literal
                && target.Count > 0
                && target[^1].Kind == InterpolatedSegmentKind.Literal)
            {
                var previous = target[^1];
                target[^1] = new InterpolatedSegment(
                    InterpolatedSegmentKind.Literal,
                    previous.Text + segment.Text
                );
            }
            else
            {
                target.Add(segment);
            }
        }
    }

    public override Expression Reduce()
    {
        var left = Left.Reduce();
        var right = Right.Reduce();

        if (left is NumberConstant lnc && right is NumberConstant rnc)
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
                case BinaryOperationType.DeepEqual:
                    return new BooleanConstant(lnc.Value == rnc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.NotEqual:
                case BinaryOperationType.DeepNotEqual:
                    return new BooleanConstant(lnc.Value != rnc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.Greater:
                    return new BooleanConstant(lnc.Value > rnc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.Less:
                    return new BooleanConstant(lnc.Value < rnc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.GreaterOrEqual:
                    return new BooleanConstant(lnc.Value >= rnc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LessOrEqual:
                    return new BooleanConstant(lnc.Value <= rnc.Value)
                        .CopyMetadata<BooleanConstant>(this);

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
                    return new BooleanConstant(lnc.Value != 0 && rnc.Value != 0)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalOr:
                    return new BooleanConstant(lnc.Value != 0 || rnc.Value != 0)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.ShiftLeft:
                    return new NumberConstant((long)lnc.Value << (int)rnc.Value)
                        .CopyMetadata<NumberConstant>(this);

                case BinaryOperationType.ShiftRight:
                    return new NumberConstant((long)lnc.Value >> (int)rnc.Value)
                        .CopyMetadata<NumberConstant>(this);
            }
        }

        if (left is StringConstant lsc && right is StringConstant rsc)
        {
            switch (Type)
            {
                case BinaryOperationType.Add:
                {
                    var isInterpolated = lsc.IsInterpolated || rsc.IsInterpolated;
                    var segments = isInterpolated
                        ? CombineStringSegments(GetStringSegments(lsc), GetStringSegments(rsc))
                        : null;

                    return new StringConstant(lsc.Value + rsc.Value, isInterpolated, segments)
                        .CopyMetadata<StringConstant>(this);
                }

                case BinaryOperationType.Equal:
                case BinaryOperationType.DeepEqual:
                    return new BooleanConstant(lsc.Value == rsc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.NotEqual:
                case BinaryOperationType.DeepNotEqual:
                    return new BooleanConstant(lsc.Value != rsc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalAnd:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalOr:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);
            }
        }

        if (left is StringConstant lsc2 && right is NumberConstant rnc2)
        {
            if (Type == BinaryOperationType.ShiftLeft)
            {
                var str = lsc2.Value;
                var amount = (int)rnc2.Value;

                if (amount >= str.Length)
                {
                    return new StringConstant(string.Empty, false)
                        .CopyMetadata<StringConstant>(this);
                }

                return new StringConstant(str[amount..], false)
                    .CopyMetadata<StringConstant>(this);
            }

            if (Type == BinaryOperationType.ShiftRight)
            {
                var str = lsc2.Value;
                var amount = str.Length - (int)rnc2.Value;

                if (amount <= 0)
                {
                    return new StringConstant(string.Empty, false)
                        .CopyMetadata<StringConstant>(this);
                }

                return new StringConstant(str.Substring(0, amount), false)
                    .CopyMetadata<StringConstant>(this);
            }
        }

        if (Type == BinaryOperationType.Add)
        {
            if (left is StringConstant lscAdd && right is ConstantExpression)
            {
                var rStr = GetConstantStringRepresentation(right);
                if (rStr != null)
                {
                    var segments = lscAdd.IsInterpolated
                        ? CombineStringSegments(
                            GetStringSegments(lscAdd),
                            [new InterpolatedSegment(InterpolatedSegmentKind.Literal, rStr)]
                        )
                        : null;

                    return new StringConstant(lscAdd.Value + rStr, lscAdd.IsInterpolated, segments)
                        .CopyMetadata<StringConstant>(this);
                }
            }
            else if (right is StringConstant rscAdd && left is ConstantExpression)
            {
                var lStr = GetConstantStringRepresentation(left);
                if (lStr != null)
                {
                    var segments = rscAdd.IsInterpolated
                        ? CombineStringSegments(
                            [new InterpolatedSegment(InterpolatedSegmentKind.Literal, lStr)],
                            GetStringSegments(rscAdd)
                        )
                        : null;

                    return new StringConstant(lStr + rscAdd.Value, rscAdd.IsInterpolated, segments)
                        .CopyMetadata<StringConstant>(this);
                }
            }
        }

        if (left is BooleanConstant lbc && right is BooleanConstant rbc)
        {
            switch (Type)
            {
                case BinaryOperationType.LogicalAnd:
                    return new BooleanConstant(lbc.Value && rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalOr:
                    return new BooleanConstant(lbc.Value || rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.BitwiseAnd:
                    return new BooleanConstant(lbc.Value & rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.BitwiseOr:
                    return new BooleanConstant(lbc.Value | rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.BitwiseXor:
                    return new BooleanConstant(lbc.Value ^ rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.Equal:
                case BinaryOperationType.DeepEqual:
                    return new BooleanConstant(lbc.Value == rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.NotEqual:
                case BinaryOperationType.DeepNotEqual:
                    return new BooleanConstant(lbc.Value != rbc.Value)
                        .CopyMetadata<BooleanConstant>(this);
            }
        }

        if (left is NilConstant && right is NilConstant)
        {
            switch (Type)
            {
                case BinaryOperationType.Equal:
                case BinaryOperationType.DeepEqual:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.NotEqual:
                case BinaryOperationType.DeepNotEqual:
                    return new BooleanConstant(false)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalAnd:
                    return new BooleanConstant(false)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalOr:
                    return new BooleanConstant(false)
                        .CopyMetadata<BooleanConstant>(this);
            }
        }

        if (left is TypeCodeConstant ltc && right is TypeCodeConstant rtc)
        {
            switch (Type)
            {
                case BinaryOperationType.Equal:
                case BinaryOperationType.DeepEqual:
                    return new BooleanConstant(ltc.Value == rtc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.NotEqual:
                case BinaryOperationType.DeepNotEqual:
                    return new BooleanConstant(ltc.Value != rtc.Value)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalAnd:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalOr:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);
            }
        }

        if (left is ConstantExpression && right is ConstantExpression)
        {
            switch (Type)
            {
                case BinaryOperationType.Equal:
                case BinaryOperationType.DeepEqual:
                    return new BooleanConstant(false)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.NotEqual:
                case BinaryOperationType.DeepNotEqual:
                    return new BooleanConstant(true)
                        .CopyMetadata<BooleanConstant>(this);

                case BinaryOperationType.LogicalAnd:
                {
                    var lt = GetConstantTruthiness(left);
                    var rt = GetConstantTruthiness(right);
                    if (lt.HasValue && rt.HasValue)
                    {
                        return new BooleanConstant(lt.Value && rt.Value)
                            .CopyMetadata<BooleanConstant>(this);
                    }
                    break;
                }

                case BinaryOperationType.LogicalOr:
                {
                    var lt = GetConstantTruthiness(left);
                    var rt = GetConstantTruthiness(right);
                    if (lt.HasValue && rt.HasValue)
                    {
                        return new BooleanConstant(lt.Value || rt.Value)
                            .CopyMetadata<BooleanConstant>(this);
                    }
                    break;
                }
            }
        }

        if (left != Left || right != Right)
        {
            return new BinaryExpression(left, right, Type)
                .CopyMetadata<BinaryExpression>(this);
        }

        return this;
    }
}