namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class NonNilAssertionExpression : Expression
{
    public Expression Operand { get; }

    public NonNilAssertionExpression(Expression operand)
    {
        Operand = operand;

        Reparent(Operand);
    }
}
