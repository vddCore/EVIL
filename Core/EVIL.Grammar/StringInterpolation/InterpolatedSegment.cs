namespace EVIL.Grammar.StringInterpolation;

using EVIL.Grammar.AST.Base;

public sealed class InterpolatedSegment
{
    public InterpolatedSegmentKind Kind { get; }
    public string Text { get; }
    public Expression? Expr { get; }

    public InterpolatedSegment(InterpolatedSegmentKind kind, string text, Expression? expr = null)
    {
        Kind = kind;
        Text = text;
        Expr = expr;
    }
}