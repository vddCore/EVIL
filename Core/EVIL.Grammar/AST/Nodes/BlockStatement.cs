using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class BlockStatement : Statement
    {
        public List<Statement> Statements { get; }

        public BlockStatement(List<Statement> statements)
        {
            Statements = statements;

            for (var i = 0; i < statements.Count; i++)
            {
                Reparent(statements[i]);
            }
        }
    }
}