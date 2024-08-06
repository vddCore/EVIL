namespace EVIL.Grammar.AST.Statements;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class ForStatement : Statement
{
    public List<Statement> Assignments { get; }
    public Expression Condition { get; }
    public List<Statement> IterationStatements { get; }
    public Statement Statement { get; }

    public ForStatement(
        List<Statement> assignments, 
        Expression condition, 
        List<Statement> iterationStatements,
        Statement statement)
    {
        Assignments = assignments;
        Condition = condition;
        IterationStatements = iterationStatements;
        Statement = statement;

        Reparent(Assignments);
        Reparent(IterationStatements);
        Reparent(Condition);
        Reparent(Statement);
    }
}