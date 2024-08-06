namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;

public sealed class WhileStatement : Statement
{
    public Expression Condition { get; }
    public Statement Statement { get; }

    public WhileStatement(Expression condition, Statement statement)
    {
        Condition = condition;
        Statement = statement;

        Reparent(Condition, Statement);
    }
}