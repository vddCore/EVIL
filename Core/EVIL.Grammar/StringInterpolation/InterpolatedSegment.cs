namespace EVIL.Grammar.StringInterpolation;

using EVIL.Grammar.AST.Base;

public sealed class InterpolatedSegment
{
    public InterpolatedSegmentKind Kind { get; }
    public string Text { get; }
    public Expression? Expression { get; }

    public InterpolatedSegment(InterpolatedSegmentKind kind, string text, Expression? expression = null)
    {
        Kind = kind;
        Text = text;
        Expression = expression;
    }
}