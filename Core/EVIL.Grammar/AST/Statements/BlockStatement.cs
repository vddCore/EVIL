namespace EVIL.Grammar.AST.Statements;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class BlockStatement : Statement
{
    public List<Statement> Statements { get; }

    public BlockStatement(List<Statement> statements)
    {
        Statements = statements;
        Reparent(Statements);
    }
}