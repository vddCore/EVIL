namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;

public sealed class DoWhileStatement : Statement
{
    public Expression Condition { get; }
    public Statement Statement { get; }

    public DoWhileStatement(Expression condition, Statement statement)
    {
        Condition = condition;
        Statement = statement;

        Reparent(Condition, Statement);
    }
}