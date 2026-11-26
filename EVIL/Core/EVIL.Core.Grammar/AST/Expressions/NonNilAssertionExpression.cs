namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

public sealed class NonNilAssertionExpression : Expression
{
    public Expression Operand { get; }

    public NonNilAssertionExpression(Expression operand)
    {
        Operand = operand;

        Reparent(Operand);
    }
}
