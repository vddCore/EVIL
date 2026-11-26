namespace EVIL.Core.Grammar.AST.Constants;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.StringInterpolation;

public sealed class StringConstant : ConstantExpression
{
    public string Value { get; }
    public bool IsInterpolated { get; }
    public IReadOnlyList<InterpolatedSegment> Segments { get; }

    public StringConstant(string value, bool isInterpolated)
        : this(value, isInterpolated, null)
    {
    }

    public StringConstant(
        string value,
        bool isInterpolated,
        IReadOnlyList<InterpolatedSegment>? segments)
    {
        Value = value;
        IsInterpolated = isInterpolated;
        Segments = segments is null 
            ? [] 
            : [..segments];

        foreach (var segment in Segments)
        {
            Reparent(segment.Expression);
        }
    }
}